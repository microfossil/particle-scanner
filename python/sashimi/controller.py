import os
from pathlib import Path

import cv2
import numpy as np

import skimage.io as skio

from sashimi.camera import Camera
from sashimi.scanner import Scanner
from sashimi.stage import Stage
from sashimi.configuration import Configuration


class Controller(object):
    def __init__(self,
                 save_dir,
                 com_port,
                 lang="en"):
        self.img_mode = 1

        self.config = Configuration.load()
        self.stage = Stage(com_port)
        self.camera = Camera(self)
        self.scanner = Scanner(self, save_dir)

        self.lang = lang

        self.quit_requested = False
        self.show_help = False
        self.take_stack_requested = False
        self.start_scan_requested = False
        self.stop_scan_requested = False
        self.time_remaining = None

        self.HOME = ord('H')
        self.SET_HOME = ord('h')

        self.FORWARD = ord('w')
        self.BACK = ord('s')
        self.LEFT = ord('a')
        self.RIGHT = ord('d')
        self.UP = ord('q')
        self.DOWN = ord('e')

        self.XFORWARD = ord('W')
        self.XBACK = ord('S')
        self.XLEFT = ord('A')
        self.XRIGHT = ord('D')
        self.XUP = ord('Q')
        self.XDOWN = ord('E')

        self.SCAN_FL = ord('j')
        self.SCAN_BR = ord('i')
        self.MOVE_SCAN_FL = ord('J')
        self.MOVE_SCAN_BL = ord('U')
        self.MOVE_SCAN_BR = ord('I')
        self.MOVE_SCAN_FR = ord('K')
        self.SCAN = ord('l')

        self.HELP1 = ord('?')
        self.HELP2 = ord('/')

        self.TAKE_STACK1 = ord('\n')
        self.TAKE_STACK2 = ord('\r')

        if self.lang == "fr":
            self.FORWARD = ord('z')
            self.BACK = ord('s')
            self.LEFT = ord('q')
            self.RIGHT = ord('d')
            self.UP = ord('q')
            self.DOWN = ord('e')

            self.XFORWARD = ord('Z')
            self.XBACK = ord('S')
            self.XLEFT = ord('Q')
            self.XRIGHT = ord('D')
            self.XUP = ord('A')
            self.XDOWN = ord('E')


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

            # Home
            elif key == self.HOME:
                self.stage.move_home(self.config.home_offset)
                print("Stage: move home")
            elif key == self.SET_HOME:
                self.config.home_offset = self.stage.position
                print("Stage: set current position to home offset")

            # Move stage
            elif key == self.FORWARD:
                self.stage.move_y(1000)
                print("Stage: move y 1mm")
            elif key == self.BACK:
                self.stage.move_y(-1000)
                print("Stage: move y -1mm")
            elif key == self.LEFT:
                self.stage.move_x(-1000)
                print("Stage: move x -1mm")
            elif key == self.RIGHT:
                self.stage.move_x(1000)
                print("Stage: move x 1mm")

            elif key == self.XFORWARD:
                self.stage.move_y(10000)
                print("Stage: move y 1mm")
            elif key == self.XBACK:
                self.stage.move_y(-10000)
                print("Stage: move y -1mm")
            elif key == self.XLEFT:
                self.stage.move_x(-10000)
                print("Stage: move x -1mm")
            elif key == self.XRIGHT:
                self.stage.move_x(10000)
                print("Stage: move 1mm")

            elif key == self.UP:
                self.stage.move_z(20)
                print("Stage: move z 20um")
            elif key == self.DOWN:
                self.stage.move_z(-20)
                print("Stage: move z -20um")
            elif key == self.XUP:
                self.stage.move_z(200)
                print("Stage: move z 200um")
            elif key == self.XDOWN:
                self.stage.move_z(-200)
                print("Stage: move z -200um")

            elif key == ord('r'):
                self.stage.poll()
                print("Stage: poll position")

            # Scan area
            elif key == self.SCAN_FL:
                self.config.scan_front_left = [self.stage.x, self.stage.y, self.stage.z]
                self.config.save()
            elif key == self.SCAN_BR:
                self.config.scan_back_right = [self.stage.x, self.stage.y, self.stage.z]
                self.config.save()

            # Move scan area
            elif key == self.MOVE_SCAN_FL:
                self.stage.goto(self.config.scan_front_left)
            elif key == self.MOVE_SCAN_BR:
                self.stage.goto(self.config.scan_back_right)
            elif key == self.MOVE_SCAN_BL:
                self.stage.goto([self.config.scan_front_left[0],self.config.scan_back_right[1],self.config.scan_front_left[2]])
            elif key == self.MOVE_SCAN_FR:
                self.stage.goto([self.config.scan_back_right[0],self.config.scan_front_left[1],self.config.scan_back_right[2]])

            # One-off take stack
            elif key == self.TAKE_STACK1 or key == self.TAKE_STACK2:
                self.take_stack_requested = True

            # Find floor
            elif key == ord('c'):
                self.scanner.find_floor()

        # Stack control
        if key == ord('{'):
            self.config.stack_step -= 20
            if self.config.stack_step < 20:
                self.config.stack_step = 20
            self.scanner.update_stack_count()
            self.config.save()
        elif key == ord('}'):
            self.config.stack_step += 20
            if self.config.stack_step > 200:
                self.config.stack_step = 200
            self.scanner.update_stack_count()
            self.config.save()
        elif key == ord('['):
            self.config.stack_height -= 100
            if self.config.stack_height < 100:
                self.config.stack_height = 100
            self.scanner.update_stack_count()
            self.config.save()
        elif key == ord(']'):
            self.config.stack_height += 100
            if self.config.stack_height > 10000:
                self.config.stack_height = 10000
            self.scanner.update_stack_count()
            self.config.save()

        # Scan
        elif key == self.SCAN:
            if self.scanner.is_scanning:
                self.scanner.is_scanning = False
            else:
                self.scanner.scan()

        # Help
        elif key == self.HELP1 or key == self.HELP2:
            self.show_help = ~self.show_help

        # Quit
        elif key == 27:
            self.config.save()
            self.quit_requested = True

    def display(self, im: np.array):
        # Reduce size of image
        im = im[::4, ::4, :].astype(np.uint8)
        # Image mode and 2, 3, 4 are red, green and blue.
        # If one of these are selected, create a new colour image just from that channel
        if self.img_mode > 1:
            im = np.repeat(im[:, :, self.img_mode - 2][..., np.newaxis], 3, axis=-1)
        # Add some black space to left of image to draw current status
        LEFT_EDGE_SIZE = 300
        im = np.pad(im, [[0, 0], [LEFT_EDGE_SIZE, 0], [0,0]])
        # Draw the status text
        if self.scanner.is_scanning:
            if self.lang == "en":
                scan_command = "Stop scanning"
            if self.lang == "fr":
                scan_command = "Arreter le scan"
        else:
            if self.lang == "en":
                scan_command = "Start scanning"
            if self.lang == "fr":
                scan_command = "Demarrer le scan"
        if self.lang == "en":
            text_status = [
                "POSITION",
                f"X: {self.stage.x}",
                f"Y: {self.stage.y}",
                f"Z: {self.stage.z}",
                f"Home: {self.config.home_offset}",
                "- - - - - - - - - - - -",
                "STACK",
                f"Height: {self.config.stack_height}um",
                f"Step: {self.config.stack_step}um",
                "- - - - - - - - - - - -",
                "SCAN",
                f"FL: {self.config.scan_front_left}",
                f"BR: {self.config.scan_back_right}",
                "- - - - - - - - - - - -",
                "COMMANDS",
                f"{scan_command}",
                "",
                ""
                f"Progress: {self.scanner.current_stack} / {self.scanner.total_stacks}"
            ]
        elif self.lang == "fr":
            text_status = [
                "POSITION",
                f"X: {self.stage.x}",
                f"Y: {self.stage.y}",
                f"Z: {self.stage.z}",
                f"Origine: {self.config.home_offset}",
                "- - - - - - - - - - - -",
                "PILE",
                f"Hauteur: {self.config.stack_height}um",
                f"Etape: {self.config.stack_step}um",
                "- - - - - - - - - - - -",
                "SCAN",
                f"AvGch: {self.config.scan_front_left}",
                f"ArDt: {self.config.scan_back_right}",
                "- - - - - - - - - - - -",
                "DEMANDES",
                f"{scan_command}",
                "",
                ""
                f"Course: {self.scanner.current_stack} / {self.scanner.total_stacks}"
            ]

        for i, t in enumerate(text_status):
            cv2.putText(im, t, (50, i * 20 + 20), cv2.FONT_HERSHEY_SIMPLEX, 0.4, (230, 230, 3055), 1, cv2.LINE_AA)

        # Draw the button text
        if self.lang == "en":
            text_button = [
                "",
                "a d",
                "w s",
                "q e",
                "h",
                "",
                "",
                "[ ]",
                "{ }",
                "",
                "",
                "j",
                "i",
                "",
                "",
                "l",
            ]
        elif self.lang == "fr":
            text_button = [
                "",
                "q d",
                "z s",
                "a e",
                "h",
                "",
                "",
                "[ ]",
                "{ }",
                "",
                "",
                "j",
                "i",
                "",
                "",
                "l",
            ]
        for i, t in enumerate(text_button):
            cv2.putText(im, t, (10, i * 20 + 20), cv2.FONT_HERSHEY_SIMPLEX, 0.4, (120, 255, 255), 1, cv2.LINE_AA)

        # Draw the help text
        if self.show_help:
            if self.lang == "en":
                text_help = [
                    "h/H: set/goto home position",
                    "w,s,a,d,q,e: forward, back, left, right, up, down",
                    "W,S,A,D,Q,E: 10 x forward, back, left, right, up, down",
                    "[ ]: -/+ stack height (100um)",
                    "{ }: -/+ stack step (20um)",
                    "j/J: set/goto scan front left",
                    "i/I: set/goto scan back right",
                    "enter: take stack",
                    "l: start/stop scan",
                    "esc: quit",
                    "?: close help",
                ]
            elif self.lang == "fr":
                text_help = [
                    "h/H: fixer/aller a la position d'origine",
                    "z,s,q,d,a,e: avant, arriere, gauche, droit, haut, bas",
                    "Z,S,Q,D,A,E: 10 x avant, arriere, gauche, droit, haut, bas"
                    "[ ]: -/+ hauteur de pile (100um)",
                    "{ }: -/+ etape de pile (20um)",
                    "j/J: fixer/aller a l'avant-gauche de la zone de scan",
                    "i/I: fixer/aller a l'arriere droit de la zone de scan",
                    "entree: faire une pile",
                    "l:  demarrer/arreter un scan",
                    "esc: quitter",
                    "?: se fermer",
                ]
        else:
            if self.lang == "en":
                text_help = ["?: show help"]
            elif self.lang == "fr":
                text_help = ["?: afficher l'aide"]
        for i, t in enumerate(text_help):
            cv2.putText(im, t, (LEFT_EDGE_SIZE+10, i * 20 + 20), cv2.FONT_HERSHEY_SIMPLEX, 0.4, (0, 255, 255), 1, cv2.LINE_AA)

        # Show the image using open CV
        cv2.imshow("im", im)

    def start(self):
        self.stage.start()
        self.camera.start()
        self.stage.move_home(self.config.home_offset)

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
