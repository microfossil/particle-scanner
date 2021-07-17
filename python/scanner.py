import os
from pathlib import Path

import cv2
from camera import Camera
from controller import Controller
from stage import Stage

import skimage.io as skio

import numpy as np


class Scanner(object):
    def __init__(self, save_dir):
        self.save_dir = save_dir
        self.stack_height = 1000
        self.stack_step = 60
        self.stack_count = None
        self.update_stack_count()

        self.stage = Stage('COM3')
        self.controller = Controller(self, self.stage)
        self.camera = Camera(self.controller)

    def update_stack_count(self):
        self.stack_count = self.stack_height // self.stack_step

    def scan(self):
        os.makedirs(self.save_dir, exist_ok=True)

    def take_stack(self):
        stack_save_path = Path(self.save_dir).joinpath(f"X{self.stage.x:06d}_Y{self.stage.y:06d}_Z{self.stage.z:06d}")
        os.makedirs(stack_save_path, exist_ok=True)
        z_orig = self.stage.z
        for i in range(self.stack_count):
            img = self.camera.latest_image()
            image_save_path = stack_save_path.joinpath(f"X{self.stage.x:06d}_Y{self.stage.y:06d}_Z{self.stage.z:06d}_{i:02d}.jpg")
            skio.imsave(str(image_save_path), img[..., ::-1], check_contrast=False, quality=90)
            self.stage.move_z(self.stack_step)
            self.show_image(img)
            self.wait_ms(100)
        img = self.camera.latest_image()
        self.show_image(img)
        self.wait_ms(100)
        self.stage.goto_z(z_orig)

    def find_floor(self):
        z_orig = self.stage.z
        self.stage.goto_z(100)
        self.wait_ms(500)
        sharpness = []
        for i in range(100):
            img = self.camera.latest_image()
            self.show_image(img)
            sh = self.measure_sharpness(img)
            sharpness.append(sh)
            print(sh)
            self.stage.move_z(20)
            self.wait_ms(200)
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

    def wait_ms(self, ms):
        cv2.waitKey(ms)

    def show_image(self, img):
        if img is None:
            return
        display_img = self.controller.display(img)
        cv2.imshow("im", display_img)

    def start(self):
        self.stage.start()
        self.camera.start()

        # Control loop
        while not self.controller.quit_requested:
            img = self.camera.latest_image()

            # Display the image
            self.show_image(img)

            # Process any key commands
            self.controller.check_for_command(50)

            # Process any requests
            if self.controller.take_stack_requested:
                self.controller.take_stack_requested = False
                self.take_stack()

        # Clean up
        cv2.destroyAllWindows()
        self.stage.stop()
        self.camera.stop()