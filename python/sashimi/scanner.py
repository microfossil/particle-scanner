import os
from pathlib import Path
import skimage.io as skio
import numpy as np
from sashimi.configuration import Configuration


class Scanner(object):
    def __init__(self, controller, save_dir):
        self.save_dir = save_dir

        self.config = controller.config

        self.stack_count = None
        self.current_stack = 0
        self.total_stacks = 0
        self.is_scanning = False

        self.update_stack_count()

        self.controller = controller
        self.stage = self.controller.stage
        self.camera = self.controller.camera


    def update_stack_count(self):
        self.stack_count = self.config.stack_height // self.config.stack_step

    def scan(self):
        self.current_stack = 0
        os.makedirs(self.save_dir, exist_ok=True)
        self.stage.goto_x(self.config.scan_front_left[0])
        self.stage.goto_y(self.config.scan_front_left[1])
        self.stage.goto_z(self.config.scan_front_left[2])
        self.stage.wait_until_position(10000)
        x_steps = (self.config.scan_back_right[0] - self.config.scan_front_left[0]) // 1000
        y_steps = (self.config.scan_back_right[1] - self.config.scan_front_left[1]) // 1000
        self.is_scanning = True
        self.total_stacks = (x_steps + 1) * (y_steps + 1)
        for yi in range(y_steps + 1):
            for xi in range(x_steps + 1):
                self.current_stack += 1
                self.stage.goto_x(self.config.scan_front_left[0] + 1000 * xi)
                self.stage.goto_y(self.config.scan_front_left[1] + 1000 * yi)
                self.stage.wait_until_position(1000)
                self.take_stack()
                if self.is_scanning is False:
                    return
        self.is_scanning = False

    def take_stack(self):
        images = []
        stack_save_path = Path(self.save_dir).joinpath(f"X{self.stage.x:06d}_Y{self.stage.y:06d}_Z{self.stage.z:06d}")
        os.makedirs(stack_save_path, exist_ok=True)
        z_orig = self.stage.z
        for i in range(self.stack_count):
            img = self.camera.latest_image()
            images.append(img)
            self.show_image(img)
            image_save_path = stack_save_path.joinpath(f"X{self.stage.x:06d}_Y{self.stage.y:06d}_Z{self.stage.z:06d}_{i:02d}.jpg")
            skio.imsave(str(image_save_path), img[..., ::-1], check_contrast=False, quality=90)
            self.stage.move_z(self.config.stack_step)
            self.wait_ms_check_input(100)
            if self.is_scanning is False:
                return
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
            sh = self.measure_sharpness(img)
            sharpness.append(sh)
            print(sh)
            self.stage.move_z(20)
            self.wait_ms_check_input(200)
        sharpness = np.asarray(sharpness)
        print(np.max(sharpness, axis=0))
        print(np.argmax(sharpness, axis=0) * 20 + 100)
        self.stage.goto_z(z_orig)

    def measure_sharpness(self, img):
        img = img[::4, ::4, ...]
        sharpness = []
        for i in range(3):
            dx = np.diff(img, axis=1)[1:, :, i]  # remove the first row
            dy = np.diff(img, axis=0)[:, 1:, i]  # remove the first column
            dnorm = np.sqrt(dx ** 2 + dy ** 2)
            sharpness.append(np.average(dnorm))
        return sharpness

    def wait_ms_check_input(self, ms):
        self.controller.check_for_command(ms)

    def show_image(self, img):
        if img is None:
            return
        self.controller.display(img)

