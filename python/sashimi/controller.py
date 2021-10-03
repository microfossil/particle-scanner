import os
from pathlib import Path

import cv2
import numpy as np

import skimage.io as skio

from sashimi.camera import Camera
from sashimi.scanner import Scanner
from sashimi.stage import Stage


class Controller(object):
    def __init__(self,
                 save_dir,
                 com_port,
                 keyboard="en"):
        self.img_mode = 1

        self.stage = Stage(com_port)
        self.camera = Camera(self)
        self.scanner = Scanner(self, save_dir)

        self.quit_requested = False
        self.show_help = False
        self.take_stack_requested = False
        self.start_scan_requested = False
        self.stop_scan_requested = False
        self.time_remaining = None

        if keyboard == "en":
            self.HOME = 'h'

            self.FORWARD = 'w'
            self.BACK = 's'
            self.LEFT = 'a'
            self.RIGHT = 'd'
            self.UP = 'q'
            self.DOWN = 'e'

            self.XFORWARD = 'W'
            self.XBACK = 'S'
            self.XLEFT = 'A'
            self.XRIGHT = 'D'
            self.XUP = 'Q'
            self.XDOWN = 'E'

            self.SCAN = "P"

            self.HELP1 = '?'
            self.HELP2 = '/'

            self.TAKE_STACK1 = '\n'
            self.TAKE_STACK2 = '\r'

    def check_for_command(self, wait_time=10):
        key = cv2.waitKey(wait_time)
        if key == -1:
            return
        print(key)

        # Commands that can only be run while not scanning
        if not self.scanner.is_scanning:
            # Image display modes
            if key == ord('1'):
                self.img_mode = 1
                print("Image: display BGR")
            elif key == ord('2'):
                self.img_mode = 2
                print("Image: display blue")
            elif key == ord('3'):
                self.img_mode = 3
                print("Image: display green")
            elif key == ord('4'):
                self.img_mode = 4
                print("Image: display red")

            # Stage control
            elif key == ord('h'):
                self.stage.move_home()
                print("Stage: move home")
            elif key == ord('w'):
                self.stage.move_y(1000)
                print("Stage: move y 1mm")
            elif key == ord('s'):
                self.stage.move_y(-1000)
                print("Stage: move y -1mm")
            elif key == ord('a'):
                self.stage.move_x(-1000)
                print("Stage: move x -1mm")
            elif key == ord('d'):
                self.stage.move_x(1000)
                print("Stage: move x 1mm")
            elif key == ord('W'):
                self.stage.move_y(10000)
                print("Stage: move y 1mm")
            elif key == ord('S'):
                self.stage.move_y(-10000)
                print("Stage: move y -1mm")
            elif key == ord('A'):
                self.stage.move_x(-10000)
                print("Stage: move x -1mm")
            elif key == ord('D'):
                self.stage.move_x(10000)
                print("Stage: move 1mm")
            elif key == ord('q'):
                self.stage.move_z(20)
                print("Stage: move z 20um")
            elif key == ord('e'):
                self.stage.move_z(-20)
                print("Stage: move z -20um")
            elif key == ord('Q'):
                self.stage.move_z(200)
                print("Stage: move z 200um")
            elif key == ord('E'):
                self.stage.move_z(-200)
                print("Stage: move z -200um")
            elif key == ord('r'):
                self.stage.poll()
                print("Stage: poll position")

            # Scan area
            elif key == ord('J'):
                self.scanner.scan_front_left = [self.stage.x, self.stage.y, self.stage.z]
            elif key == ord('I'):
                self.scanner.scan_back_right = [self.stage.x, self.stage.y, self.stage.z]

            # One-off stack
            elif key == ord(self.TAKE_STACK1) or key == ord(self.TAKE_STACK2):
                self.take_stack_requested = True

            # Find floor
            elif key == ord('c'):
                self.scanner.find_floor()

        # Stack control
        if key == ord('{'):
            self.scanner.stack_step -= 20
            if self.scanner.stack_step < 20:
                self.scanner.stack_step = 20
            self.scanner.update_stack_count()
        elif key == ord('}'):
            self.scanner.stack_step += 20
            if self.scanner.stack_step > 200:
                self.scanner.stack_step = 200
            self.scanner.update_stack_count()
        elif key == ord('['):
            self.scanner.stack_height -= 100
            if self.scanner.stack_height < 100:
                self.scanner.stack_height = 100
            self.scanner.update_stack_count()
        elif key == ord(']'):
            self.scanner.stack_height += 100
            if self.scanner.stack_height > 10000:
                self.scanner.stack_height = 10000
            self.scanner.update_stack_count()

        # Scan
        elif key == ord(self.SCAN):
            if self.scanner.is_scanning:
                self.scanner.is_scanning = False
            else:
                self.scanner.scan()

        # Help
        elif key == ord(self.HELP1) or key == ord(self.HELP2):
            self.show_help = ~self.show_help

        # Quit
        elif key == 27:
            self.quit_requested = True

    def display(self, im: np.array):
        # Reduce size of image
        im = im[::4, ::4, :].astype(np.uint8)
        # Image mode and 2, 3, 4 are red, green and blue.
        # If one of these are selected, create a new colour image just from that channel
        if self.img_mode > 1:
            im = np.repeat(im[:, :, self.img_mode - 2][..., np.newaxis], 3, axis=-1)
        # Add some black space to left of image to draw current status
        LEFT_EDGE_SIZE = 256
        im = np.pad(im, [[0, 0], [LEFT_EDGE_SIZE, 0], [0,0]])
        # Draw the status text
        if self.scanner.is_scanning:
            scan_command = "Stop scanning"
        else:
            scan_command = "Start scanning"
        text_status = [
            "POSITION",
            f"X: {self.stage.x}",
            f"Y: {self.stage.y}",
            f"Z: {self.stage.z}",
            "- - - - - - - - - - - -",
            "STACK",
            f"Height: {self.scanner.stack_height}um",
            f"Step: {self.scanner.stack_step}um",
            "- - - - - - - - - - - -",
            "SCAN",
            f"Front left:",
            f"{self.scanner.scan_front_left}",
            f"Back right:",
            f"{self.scanner.scan_back_right}",
            "- - - - - - - - - - - -",
            "COMMANDS",
            f"{scan_command}",
            "",
            ""
            f"{self.scanner.current_stack} / {self.scanner.total_stacks}"

        ]
        for i, t in enumerate(text_status):
            cv2.putText(im, t, (50, i * 20 + 20), cv2.FONT_HERSHEY_SIMPLEX, 0.4, (230, 230, 3055), 1, cv2.LINE_AA)

        # Draw the button text
        text_button = [
            "",
            "a d",
            "w s",
            "q e",
            "",
            "",
            "[ ]",
            "{ }",
            "",
            "",
            "",
            "J",
            "",
            "I",
            "",
            "",
            "P",
        ]
        for i, t in enumerate(text_button):
            cv2.putText(im, t, (10, i * 20 + 20), cv2.FONT_HERSHEY_SIMPLEX, 0.4, (120, 255, 255), 1, cv2.LINE_AA)

        # Draw the help text
        if self.show_help:
            text_help = [
                "h: go to home position",
                "w,a,s,d,q,e: forward, left, back, right, up, down",
                "W,A,S,D,Q,E: 10 x forward, left, back, right, up, down",
                "[ ]: increase / decrease stack height (100um)",
                "{ }: increase / decrease stack step (20um)",
                "enter: take stack",
                "P: start / stop scan",
                "escape: quit",
                "?: close help",
            ]
        else:
            text_help = ["?: show help"]
        for i, t in enumerate(text_help):
            cv2.putText(im, t, (LEFT_EDGE_SIZE+10, i * 20 + 20), cv2.FONT_HERSHEY_SIMPLEX, 0.4, (0, 255, 255), 1, cv2.LINE_AA)

        # Show the image using open CV
        cv2.imshow("im", im)

    def start(self):
        self.stage.start()
        self.camera.start()
        self.stage.move_home()
        self.scanner.scan_front_left = [self.stage.x, self.stage.y, self.stage.z]
        self.scanner.scan_back_right = [self.stage.x+50000, self.stage.y+75000, self.stage.z]

        # Control loop
        while not self.quit_requested:
            img = self.camera.latest_image()

            # Display the image
            if img is not None:
                self.display(img)

            # Process any key commands
            self.check_for_command(50)

        # Clean up
        cv2.destroyAllWindows()
        self.stage.stop()
        self.camera.stop()
