import os
import multiprocessing as mp
from dataclasses import dataclass, field
from enum import Enum
from typing import List, Optional

import skimage.io as skio
import numpy as np
import datetime as dt
from shutil import rmtree
from pathlib import Path

from PySide6.QtCore import QObject, Signal, QTimer, Slot
from PySide6.QtWidgets import QApplication

from sashimi import utils, helicon_stack
from sashimi.stage import Stage


# TODO: make an ETA function


def clip(n, mini=0, maxi=None):
    if n < mini:
        return n
    if maxi is None or n <= maxi:
        return n
    return n
    
    
def measure_sharpness(img):
    img = img[::4, ::4, ...]
    sharpness = []
    for i in range(3):
        dx = np.diff(img, axis=1)[1:, :, i]  # remove the first row
        dy = np.diff(img, axis=0)[:, 1:, i]  # remove the first column
        dnorm = np.sqrt(dx ** 2 + dy ** 2)
        sharpness.append(np.average(dnorm))
    return sharpness


def remove_folder(folder):
    for files in os.listdir(folder):
        path = os.path.join(folder, files)
        try:
            rmtree(path)
        except OSError:
            os.remove(path)


def s2hms(s: int) -> [int, int, int]:
    m = s // 60
    s = s - 60 * m
    h = m // 60
    m = m - 60 * h
    return h, m, s


@dataclass
class ScanZone:
    FL: List[int]
    BR: List[int]
    BL_Z: int
    Z_corrections: List[int]


@dataclass
class ScanConfiguration:

    save_dir: Path
    scan_name: str
    overwrite: bool = False

    scan_zones: List[ScanZone] = field(default_factory=list)
    exposure_times: Optional[List[int]] = None
    z_margin: int = 200
    lowest_z: int = None

    stack_height: int = 2000
    stack_step: int = 60
    step_X: int = 1700
    step_Y: int = 1700

    remove_raw_images: bool = True


class ScanningPhase(Enum):
    IDLE = 0
    INITIALIZING = 1
    SCANNING = 2
    FINISHED = 3
    CANCELLED = 4

@dataclass
class ScannerState:
    phase: ScanningPhase = ScanningPhase.IDLE

    num_zones: int = 0
    num_stacks: int = 0
    num_images: int = 0

    steps_x: int = 0
    steps_y: int = 0

    zone_idx: int = 0
    stack_idx: int = 0
    image_idx: int = 0

    image_x: int = 0
    image_y: int = 0
    image_z: int = 0


class Scanner(QObject):
    state_changed = Signal(ScannerState)
    request_exposure_change = Signal(int)

    def __init__(self,
                 config: ScanConfiguration,
                 stage: Stage):
        super().__init__()
        self.state = ScannerState()
        self.config = config
        self.stage = stage

        # Queue for parallel processing
        self.queue = None
        self.parallel_process = None

        # Timer to run scan
        self.timer = QTimer()
        self.timer.timeout.connect(self.scan_loop)
        self.timer.setInterval(100)
        self.timer.start()

        self.cancellation_flag = False

        self.latest_camera_image = None


    # Helpers
    def _current_zone(self) -> ScanZone:
        return self.config.scan_zones[self.state.zone_idx]
        


    def _get_corrected_z(self, dx, dy):
        current_zone = self._current_zone()
        if self.config.lowest_z:
            # 'Dumb-but-works' correction
            new_z = self._lowest_corner_of_zone()
        else:
            # 'Smart' correction
            dz_dx, dz_dy = current_zone.Z_corrections
            z_correction = int(dz_dx * dx + dz_dy * dy)
            new_z = current_zone.FL[2] + z_correction
        return clip(new_z - self.config.z_margin)

    def _step_num_xy(self, scan: ScanZone) -> (int, int):
        x_steps = 1 + (scan.BR[0] - scan.FL[0]) // self.config.step_X
        y_steps = 1 + (scan.BR[1] - scan.FL[1]) // self.config.step_Y
        return x_steps, y_steps

    # State
    def _update_counts(self):
        # Number of zones
        self.state.num_zones = len(self.config.scan_zones)
        # Number of stacks
        num_stacks = 0
        for scan in self.config.scan_zones:
            x_steps, y_steps = self._step_num_xy(scan)
            num_stacks += x_steps * y_steps
        self.state.num_stacks = max(len(self.config.exposure_times), 1) * num_stacks
        # Number of images in stack
        self.state.num_images = self.config.stack_height // self.config.stack_step

    def _update_state(self):
        # Update
        self.state_changed.emit(self.state)
        self._event_loop()

    # Make QT work
    def _event_loop(self):
        QApplication.processEvents()

    # Start
    Slot()
    def start(self):
        self.scan()

    # Image
    Slot(np.ndarray)
    def update_image(self, img):
        self.latest_camera_image = img

    def change_exposure(self, exp):
        self.request_exposure_change.emit(exp)
        self._event_loop()

    # Cancel
    Slot()
    def cancel(self):
        self.cancellation_flag = True

    def _cancel(self):
        self.state.phase = ScanningPhase.CANCELLED
        self.parallel_process.terminate()
        self.parallel_process.join()
        self._update_state()

    def _is_cancelled(self):
        if self.cancellation_flag:
            self._cancel()
            return True
        return False

    def scan(self):
        # Don't start if no zones
        if len(self.config.scan_zones) == 0:
            return

        self.cancellation_flag = False

        # Initialise state
        self.state.is_scanning = True
        self.state.zone_idx = 0
        self.state.stack_idx = 0
        self.state.image_idx = 0
        self._update_counts()
        self._update_state()

        # Create scan, removing existing if necessary
        scan_dir = Path(self.config.save_dir) / self.config.scan_name
        if scan_dir.exists() and self.config.overwrite:
            utils.remove_folder(scan_dir)
        Path(self.config.save_dir).mkdir(exist_ok=True)

        # Folder for focus stack images
        stack_dir = scan_dir / "f_stack"
        stack_dir.mkdir(exist_ok=True)

        # Stack errors
        error_logs = scan_dir / 'error_logs.txt'
        if error_logs.exists():
            os.remove(error_logs)

        # Parallel stack command queue
        self.queue = mp.Queue()
        arguments = (self.queue, error_logs, self.config.exposure_times, self.config.remove_raw_images)
        self.parallel_process = mp.Process(target=helicon_stack.parallel_stack, args=arguments)
        self.parallel_process.start()
        self.state.phase = ScanningPhase.SCANNING
        self._update_state()

        for zone_idx, scan_zone in enumerate(self.config.scan_zones):
            self.state.zone_idx = 0
            self._update_state()

            self.stage.goto(scan_zone.FL)
            self.stage.wait_until_position(10000)
            self.state.steps_x, self.state.steps_y = self._step_num_xy(scan_zone)
            self._update_state()

            # Start scanning
            self.state.stack_idx = 0
            for yi in range(self.state.steps_y):
                for xi in range(self.state.steps_x):

                    if self._is_cancelled():
                        return

                    steps_x, steps_y = self.state.steps_x, self.state.steps_y
                    fl = scan_zone.FL

                    dx, dy = steps_x * xi, steps_y * yi
                    du = [fl[0] + dx, fl[1] + dy, fl[2]]

                    self.stage.goto(du)
                    self.stage.wait_until_position(1000)

                    if self._is_cancelled():
                        return

                    # Stack folder
                    xy_folder = stack_dir / f"X{self.stage.x//10:05d}_Y{self.stage.y//10:05d}"
                    os.makedirs(xy_folder, exist_ok=True)

                    # Exposure loop
                    for exposure in self.config.exposure_times:
                        self.change_exposure(exposure)
                        exp_folder = xy_folder / f"E{exposure}"
                        os.makedirs(exp_folder, exist_ok=True)

                        # Go to the correct Z
                        z_orig = self.stage.z
                        self.stage.goto_z(self._get_corrected_z(dx, dy))
                        self.stage.wait_until_position(1000)

                        if self._is_cancelled():
                            return

                        # Take a stavh
                        self.state.image_idx = 0
                        for image_idx in range(self.state.num_images):
                            self.state.image_idx = stack_idx
                            self._update_state()
                            self._take_image(exp_folder)
                            self.stage.move_z(self.config.stack_step)
                            self.stage.wait_until_position(100)

                self.current_pic_count += 1
                if self.check_for_escape():
                    print('escaping take_stack()')
                    return
                # set exposure and take a picture
                self.camera.set_exposure(exp)
                img = self.wait_until_exposure(exp, 300)
                # self.show_image(img)
                
                # save the picture
                if self.multi_exp is not None:
                    save_path = xy_folder.joinpath(f"E{exp}")
                else:
                    save_path = xy_folder
                save_path = save_path.joinpath(f"X{self.stage.x//10:05d}_"
                                               f"Y{self.stage.y//10:05d}_"
                                               f"Z{self.stage.z//10:05d}.jpg")
                skio.imsave(str(save_path), img[..., ::-1], check_contrast=False, quality=90)
            
            self.stage.move_z(self.config.stack_step)
            self.stage.wait_until_position(100)
        
        if self.auto_f_stack:
            self.queue.put((str(xy_folder), str(self.fs_folder)))

        self.camera.set_exposure(exp_values[0])
        self.stage.goto_z(z_orig)
        self.stage.wait_until_position(50 * self.stack_count)
        
    # def find_floor(self):
    #     z_orig = self.stage.z
    #     self.stage.goto_z(100)
    #     self.stage.wait_until_position(500)
    #     sharpness = []
    #     for i in range(100):
    #         img = self.camera.latest_image()
    #         self.show_image(img)
    #         sh = measure_sharpness(img)
    #         sharpness.append(sh)
    #         print(sh)
    #         self.stage.move_z(20)
    #         self.stage.wait_until_position(200)
    #     sharpness = np.asarray(sharpness)
    #     print(np.max(sharpness, axis=0))
    #     print(np.argmax(sharpness, axis=0) * 20 + 100)
    #     self.stage.goto_z(z_orig)
    
    def wait_until_exposure(self, exp, ms):
        img = None
        for i in range(ms//self.frame_duration_ms):
            img, img_exp = self.camera.latest_image(with_exposure=True)
            if exp == img_exp:
                return img
            # else:
            #     self.controller.display(img)
            #     self.controller.wait(display=False)
        print(f'desired exposure was not reached in {ms}ms')
        return img

    def show_image(self, img):
        if img is None:
            return
        self.controller.update_image(img)
    
    def check_for_escape(self):
        if self.is_multi_scanning and not self.controller.quit_requested:
            return False
        if self.auto_quit:
            self.controller.quit_requested = True
        self.is_multi_scanning = False
        self.controller.interrupt_flag = True
        return True

    def make_scan_summary(self):
        summary_path = self.controller.save_dir.joinpath('summary.txt')
        with open(summary_path, mode='x') as summary:
            if self.controller.interrupt_flag:
                summary.write('///////////THE SCANS WERE INTERRUPTED BEFORE FINISHING!!!///////////\n\n')
            summary.write('This is the summary of this multi-scan folder.\n'
                          'Here are some parameters :')
            param_list = [
                'save_dir',
                'language',
                'layout',
                'auto_f_stack',
                'remove_raw',
                'auto_quit',
                'lowest_z',
                'exposure (µs)',
                'stack height (µm)',
                'XY_step (µm)',
                'stack_step (µm)'
            ]
            for param in param_list:
                summary.write(f"{param} = {self.summary[param]}\n")
            dates = self.summary['scan_dates']
            deltas = [dates[n+1] - dates[n] for n in range(len(dates) - 1)]
            total_pics = 0
            
            if self.multi_exp is None:
                exp_count = 1
            else:
                exp_count = len(self.multi_exp)
            summary.write('Here are statistics about the scans.')
            for n in range(len(deltas)):
                scan = self.config.scans[n]
                steps_x, steps_y = self._step_num_xy(scan)
                stack_nbr = steps_x * steps_y
                pic_nbr = stack_nbr * self.stack_count
                total_pics += pic_nbr
                h, m, s = s2hms(deltas[n].seconds)
                summary.write(
                    f"scan{n + 1} started at {dates[n]}, lasted {h}h {m}min {s}s and took :\n"
                    f"{stack_nbr} stacks x {exp_count} exposures x {self.stack_count} heights = {pic_nbr} pictures.\n\n"
                )
            delta = dates[-1] - dates[0]
            h, m, s = s2hms(int(delta.total_seconds()))
            summary.write(f'Overall, the task ended at {dates[-1]} and lasted {h}h {m}min and {s}s.\n')
