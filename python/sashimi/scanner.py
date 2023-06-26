import os
from shutil import rmtree
from pathlib import Path
import multiprocessing as mp
import skimage.io as skio
import numpy as np
from sashimi.helicon_stack import single_stack

# TODO: make an ETA function
# TODO: use package pillow-heif to compress raw stacks (and exposures?)


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


class Scanner(object):
    def __init__(self, controller):
        # instances
        self.controller = controller
        self.stage = self.controller.stage
        self.camera = self.controller.camera
        self.config = self.controller.config

        self.frame_duration_ms = self.controller.frame_duration_ms
        self.auto_f_stack = self.controller.auto_f_stack
        self.remove_raw = self.controller.remove_raw
        self.auto_quit = self.controller.auto_quit
        self.save_dir = self.controller.save_dir
        self.scan_dir = self.save_dir
        self.selected_scan = self.controller.selected_scan
        self.multi_exp = self.controller.multi_exp
        self.fs_folder = self.save_dir.joinpath("f_stacks")
        
        # parameters and variables
        if self.multi_exp:
            self.fs_exp_folders = [self.fs_folder.joinpath(f"E{exp}") for exp in self.multi_exp]
        self.X_STEP = 1700
        self.Y_STEP = 1700
        self.stack_count = None
        self.current_stack = 0
        self.total_stacks = 0
        self.current_pic_count = 0
        self.total_pic_count = 0
        self.is_multi_scanning = False
        self.parallel_process = None
        self.update_stack_count()
        self.update_total_pic_count()
        
    def lowest_corner(self) -> int:
        current_scan = self.selected_scan()
        fl = current_scan['FL']
        br = current_scan['BR']
        blz = current_scan['BL_Z']
        flz = fl[2]
        brz = br[2]
        frz = flz - brz + blz
        mini = min((blz, brz, flz, frz))
        if mini < 0:
            mini = 0
        return mini
    
    def get_corrected_z(self, dx, dy):
        if self.controller.lowest_z:
            # 'Dumb-but-works' correction
            new_z = self.lowest_corner()
        else:
            # 'Smart' correction
            dz_dx, dz_dy = self.selected_scan()['Z_corrections']
            z_correction = int(dz_dx * dx + dz_dy * dy)
            new_z = self.selected_scan()['FL'][2] + z_correction
        return clip(new_z)
        
    def update_stack_count(self):
        self.stack_count = self.config.stack_height // self.config.stack_step

    def update_total_pic_count(self):
        if self.multi_exp:
            pps = self.stack_count * len(self.multi_exp)
        else:
            pps = self.stack_count
        
        total_stacks = 0
        for scan in self.config.scans:
            x_steps, y_steps = self.step_nbr_xy(scan)
            total_stacks += x_steps * y_steps
        
        self.total_pic_count = pps * total_stacks
    
    def step_nbr_xy(self, scan) -> (int, int):
        x_steps = 1 + (scan['BR'][0] - scan['FL'][0]) // self.X_STEP
        y_steps = 1 + (scan['BR'][1] - scan['FL'][1]) // self.Y_STEP
        return x_steps, y_steps
    
    def multi_scan(self):
        self.is_multi_scanning = True
        self.controller.selected_scan_number = 1
        self.controller.interrupt_flag = False
        self.current_pic_count = 0
        self.update_total_pic_count()
        os.makedirs(self.save_dir, exist_ok=True)

        if self.auto_f_stack:
            mp.set_start_method("spawn")
            queue = mp.Queue()
            self.parallel_process = mp.Process(target=single_stack, args=(queue, self.multi_exp))
            self.parallel_process.start()
            
        for n in range(len(self.config.scans)):
            if not self.is_multi_scanning:
                break
                
            scan_name = f"scan{n + 1}"
            scan_dir = Path(self.save_dir).joinpath(scan_name)
            os.makedirs(scan_dir)
            self.controller.selected_scan_number = n + 1
            
            self.scan(scan_dir)
        
        if self.auto_f_stack:
            if self.controller.quit_requested or self.controller.interrupt_flag:
                self.parallel_process.kill()
            else:
                self.parallel_process.join()

        self.is_multi_scanning = False
        if self.auto_quit:
            # in case of user interruption:
            self.controller.interrupt_flag = True if self.controller.quit_requested else False
            self.controller.quit_requested = True

    def scan(self, scan_dir):
        selected_scan = self.controller.selected_scan()
        fl = selected_scan['FL']
        br = selected_scan['BR']
        assert (br[0] > fl[0])
        assert (br[1] > fl[1])
        
        os.makedirs(scan_dir, exist_ok=True)
        
        self.stage.goto(fl)
        self.stage.wait_until_position(10000)
        x_steps, y_steps = self.step_nbr_xy(selected_scan)
        self.total_stacks = x_steps * y_steps
        
        # Start scanning
        self.current_stack = 0
        for yi in range(y_steps):
            for xi in range(x_steps):
                self.current_stack += 1

                if self.check_for_escape():
                    print('escaping scan()')
                    return
                
                dx, dy = self.X_STEP * xi, self.Y_STEP * yi
                du = [fl[0] + dx, fl[1] + dy, fl[2]]
                self.stage.goto(du)
                self.stage.wait_until_position(1000)
                self.take_stack(dx, dy, scan_dir)

    def take_stack(self, dx, dy, scan_dir):
        # Create directory to save stack
        xy_folder = Path(scan_dir).joinpath(f"X{self.stage.x//10:05d}_Y{self.stage.y//10:05d}")
        os.makedirs(xy_folder, exist_ok=True)
        if self.multi_exp:
            for exp in self.multi_exp:
                os.makedirs(xy_folder.joinpath(f"E{exp}"), exist_ok=True)
        
        z_orig = self.stage.z
        self.stage.goto_z(self.get_corrected_z(dx, dy))
        self.stage.wait_until_position(1000)
        
        exp_values = self.multi_exp if self.multi_exp else (self.config.exposure_time,)
        for i in range(self.stack_count):
            for exp in exp_values:
                self.current_pic_count += 1
                if self.check_for_escape():
                    print('escaping take_stack()')
                    return
                # set exposure and take a picture
                self.camera.set_exposure(exp)
                img = self.wait_until_exposure(exp, 300)
                self.show_image(img)
                
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
            self.parallel_process.put((xy_folder, self.fs_folder))

        self.camera.set_exposure(exp_values[0])
        self.stage.goto_z(z_orig)
        self.stage.wait_until_position(50 * self.stack_count)
        
    def find_floor(self):
        z_orig = self.stage.z
        self.stage.goto_z(100)
        self.stage.wait_until_position(500)
        sharpness = []
        for i in range(100):
            img = self.camera.latest_image()
            # self.show_image(img)
            sh = measure_sharpness(img)
            sharpness.append(sh)
            print(sh)
            self.stage.move_z(20)
            self.stage.wait_until_position(200)
        sharpness = np.asarray(sharpness)
        print(np.max(sharpness, axis=0))
        print(np.argmax(sharpness, axis=0) * 20 + 100)
        self.stage.goto_z(z_orig)
    
    def wait_until_exposure(self, exp, ms):
        img = None
        for i in range(ms//self.frame_duration_ms):
            img, img_exp = self.camera.latest_image(with_exposure=True)
            if exp == img_exp:
                
                return img
            else:
                self.controller.display(img)
                self.controller.wait(display=False)
        print(f'desired exposure was not reached in {ms}ms')
        return img

    def show_image(self, img):
        if img is None:
            return
        self.controller.display(img)
    
    def check_for_escape(self):
        if self.is_multi_scanning and not self.controller.quit_requested:
            return False
        if self.auto_quit:
            self.controller.quit_requested = True
        self.is_multi_scanning = False
        return True

    # def make_scan_summary(self):
        # creates and saves a .txt file in the save directory that
        # sums up the settings of the scan
        # gives the duration of the scan + date & time at which the scan started and ended
        # the approximate space taken by all the pictures taken and created
        # if the scan was interrupted
