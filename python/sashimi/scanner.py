import os
import time
from pathlib import Path
import skimage.io as skio
import numpy as np
from sashimi.helicon_stack import stack_from_to, stack_for_multiple_exp

# TODO: fix the bug crashing the program if a multi scan of 1 scan ends

# TODO: fix lowest_corner

# TODO: auto z_correction

# TODO: add an update_total_stacks_nbr() function
# TODO: measure the time needed to :
#  - take all the pictures of a stack
#  - stack these pictures together with Helicon Focus
# TODO: make an ETA function


def measure_sharpness(img):
    img = img[::4, ::4, ...]
    sharpness = []
    for i in range(3):
        dx = np.diff(img, axis=1)[1:, :, i]  # remove the first row
        dy = np.diff(img, axis=0)[:, 1:, i]  # remove the first column
        dnorm = np.sqrt(dx ** 2 + dy ** 2)
        sharpness.append(np.average(dnorm))
    return sharpness


class Scanner(object):
    def __init__(self, controller):
        self.controller = controller
        self.stage = self.controller.stage
        self.camera = self.controller.camera
        self.config = self.controller.config
        
        self.auto_f_stack = self.controller.auto_f_stack
        
        self.auto_quit = self.controller.auto_quit
        self.save_dir = self.controller.save_dir
        self.scan_dir = self.save_dir
        self.scans = self.config.scans
        
        self.selected_scan = self.controller.selected_scan
        self.stack_count = None
        self.current_stack = 0
        self.total_stacks = 0
        
        self.is_scanning = False
        self.is_multi_scanning = False
        self.multi_exp = self.controller.multi_exp
        self.fs_folder = self.save_dir.joinpath("f_stacks")
        
        self.fs_exp_folders = [self.fs_folder.joinpath(f"E{exp}") for exp in self.multi_exp]
        self.update_stack_count()
        self.reposition_offset = self.controller.reposition_offset

        self.X_STEP = 1700
        self.Y_STEP = 1700

    def lowest_corner(self) -> int:
        current_scan = self.selected_scan()
        fl = current_scan['FL']
        br = current_scan['BR']
        blz = current_scan['BL_Z']
        flz = fl[2]
        brz = br[2]
        frz = flz - brz + blz
        mini = min(blz, brz, flz, frz)
        
        if mini < 0:
            mini = 0
        
        return mini
    
    def update_stack_count(self):
        self.stack_count = self.config.stack_height // self.config.stack_step
    
    def step_nbr_xy(self) -> (int, int):
        zone = self.selected_scan()
        x_steps = (zone['BR'][0] - zone['FL'][0]) // self.X_STEP
        y_steps = (zone['BR'][1] - zone['FL'][1]) // self.Y_STEP
        return x_steps, y_steps

    def scan(self):
        # Reset the stack
        self.current_stack = 0
        
        # Create directory to store images
        os.makedirs(self.scan_dir, exist_ok=True)

        selected_scan = self.controller.selected_scan()
        
        # Move to the starting position
        self.stage.goto(selected_scan['FL'])
        self.stage.wait_until_position(10000)
        
        # Calculate the number of steps needed
        x_steps, y_steps = self.step_nbr_xy()
        self.total_stacks = (x_steps + 1) * (y_steps + 1)
        
        # Start scanning
        self.is_scanning = True
        for yi in range(y_steps + 1):
            for xi in range(x_steps + 1):
                if (not self.is_scanning) or (not self.is_multi_scanning):
                    print("escape !")
                    self.is_multi_scanning = False
                    return
                
                self.current_stack += 1
                dx, dy = self.X_STEP * xi, self.Y_STEP * yi
                self.stage.goto_z(selected_scan['FL'][2])
                self.stage.goto_x(selected_scan['FL'][0] + dx)
                self.stage.goto_y(selected_scan['FL'][1] + dy)
                self.stage.wait_until_position(1000)
                self.wait_ms_check_input(300)
                
                self.take_stack(dx, dy)
        self.is_scanning = False

    def multi_scan(self):
        self.is_multi_scanning = True
        self.controller.selected_scan_number = 1
        os.makedirs(self.save_dir, exist_ok=True)
        # os.makedirs(self.fs_folder, exist_ok=True)
        # for folder in self.fs_exp_folders:
        #     os.makedirs(folder, exist_ok=True)

        for n, path in enumerate(self.scans):
            scan_name = f"scan{n + 1}"
            if not self.is_multi_scanning:
                return

            self.controller.selected_scan_number = n + 1
            self.stage.goto(self.selected_scan()['FL'])
            self.wait_ms_check_input(5000)

            self.scan_dir = Path(self.save_dir).joinpath(scan_name)

            self.scan()
            
            if self.auto_f_stack:
                self.focus_stack(scan_name)

        self.is_multi_scanning = False
        
        if self.auto_quit:
            # in case of user interruption:
            if self.controller.quit_requested:
                self.controller.interrupt_flag = True
                return
    
            self.controller.quit_requested = True
            return

    def focus_stack(self, scan_name):
        if not self.is_multi_scanning:
            return
        scan_fs_dir = self.fs_folder.joinpath(scan_name)

        if self.multi_exp is None:
            stack_from_to(self.scan_dir, scan_fs_dir)
        else:
            stack_for_multiple_exp(self.scan_dir, self.fs_folder, self.multi_exp)

    def take_stack(self, dx, dy):
        images = []
        # Create directory to save stack
        stack_folder = Path(self.scan_dir).joinpath(f"X{self.stage.x:06d}_Y{self.stage.y:06d}")
        os.makedirs(stack_folder, exist_ok=True)

        if self.multi_exp:
            for exp in self.multi_exp:
                os.makedirs(stack_folder.joinpath(f"E{exp}"), exist_ok=True)

        z_orig = self.stage.z
        
        if self.controller.lowest_z:  # Dumb-but-works correction
            self.stage.goto_z(self.lowest_corner())
            self.wait_ms_check_input(100)
        else:  # Smart correction
            dz_dx, dz_dy = self.selected_scan()['Z_corrections']
            z_correction = int(dz_dx * dx + dz_dy * dy)
            self.stage.move_z(z_correction)
            self.wait_ms_check_input(100)
        
        # Take the stack
        print(z_orig)

        if self.config.top_down:  # Reposition the camera with downward travel to reduce the tilting of the camera
            self.stage.move_z(self.config.stack_height + self.reposition_offset)
            self.stage.move_z(-self.reposition_offset)
        
        if self.multi_exp:
            exp_values = self.multi_exp
        else:
            exp_values = (self.config.exposure_time,)

        for i in range(self.stack_count):
            # Grab image and add to list
            for exp in exp_values:
                # Exit if scanning was stopped
                if (not self.is_scanning) or (not self.is_multi_scanning):
                    print("escape ! ! !")
                    self.is_scanning = False
                    self.is_multi_scanning = False
                    if self.multi_exp is not None:
                        self.controller.quit_requested = True
                    return

                self.camera.set_exposure(exp)
                chrono = -time.perf_counter()
                self.wait_ms_check_input(300)
                chrono += time.perf_counter()
                print(chrono)
                img = self.camera.latest_image()
                images.append(img)
                self.show_image(img)

                if self.multi_exp:
                    image_save_path = stack_folder.joinpath(f"E{exp}", f"X{self.stage.x:06d}_"
                                                                       f"Y{self.stage.y:06d}_"
                                                                       f"Z{self.stage.z:06d}.jpg")
                else:
                    image_save_path = stack_folder.joinpath(f"X{self.stage.x:06d}_"
                                                            f"Y{self.stage.y:06d}_"
                                                            f"Z{self.stage.z:06d}.jpg")

                skio.imsave(str(image_save_path), img[..., ::-1], check_contrast=False, quality=90)

            # Move down instead of up if top_down is True
            if self.config.top_down:
                self.stage.move_z(-self.config.stack_step)
            else:
                self.stage.move_z(self.config.stack_step)

        # Return to base Z coordinate
        img = self.camera.latest_image()
        self.show_image(img)
        self.wait_ms_check_input(100)
        self.stage.goto_z(z_orig)
        self.wait_ms_check_input(50 * self.stack_count)

    def find_floor(self):
        z_orig = self.stage.z
        self.stage.goto_z(100)
        self.wait_ms_check_input(500)
        sharpness = []
        for i in range(100):
            img = self.camera.latest_image()
            self.show_image(img)
            sh = measure_sharpness(img)
            sharpness.append(sh)
            print(sh)
            self.stage.move_z(20)
            self.wait_ms_check_input(200)
        sharpness = np.asarray(sharpness)
        print(np.max(sharpness, axis=0))
        print(np.argmax(sharpness, axis=0) * 20 + 100)
        self.stage.goto_z(z_orig)

    def wait_ms_check_input(self, ms):
        self.controller.check_for_command(ms)

    def show_image(self, img):
        if img is None:
            return
        self.controller.display(img)
