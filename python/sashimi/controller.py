import cv2
import numpy as np
from sashimi.camera import Camera
from sashimi.scanner import Scanner
from sashimi.stage import Stage
from sashimi.configuration import Configuration
from sashimi.keyboard import Keyboard

# TODO: fix offset in UI
# TODO: Add save/load config files


class Controller(object):
    def __init__(self,
                 save_dir,
                 com_port,
                 lang="en",
                 layout='QWERTY',
                 reposition_offset=1000,
                 auto_f_stack=True,
                 auto_quit=False,
                 multi_exp=False,
                 lowest_z=False):
        
        self.config = Configuration.load()
        self.img_mode = 1
        self.lang = lang
        self.layout = layout
        self.reposition_offset = reposition_offset
        self.auto_f_stack = auto_f_stack
        self.auto_quit = auto_quit
        self.multi_exp = multi_exp
        
        self.lowest_z = lowest_z

        self.interrupt_flag = False
        self.selected_scan_number = 1
        self.scans = self.config.scans
        self.save_dir = save_dir

        self.stage = Stage(self, com_port)
        self.camera = Camera(self)
        self.scanner = Scanner(self)
        self.keyboard = Keyboard(self.layout)

        self.quit_requested = False
        self.show_help = False
        self.take_stack_requested = False
        self.start_scan_requested = False
        self.stop_scan_requested = False
        self.time_remaining = None

    def selected_scan(self):
        return self.scans[self.selected_scan_number - 1]

    def permanent_commands(self, key):
        kb = self.keyboard
        
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

        # Stack step size commands
        elif key == ord('{'):
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

        elif key == kb.FLIP_STACK_ORDER:
            self.config.top_down = ~self.config.top_down
            print("Stack will be taken from the " + ("top down." if self.config.top_down else "bottom up."))
        # Exposure
        elif key == kb.EXPOSURE_UP:
            self.config.exposure_time += 100
            if self.config.exposure_time > 50000:
                self.config.exposure_time = 50000
            self.camera.set_exposure(self.config.exposure_time)
        elif key == kb.EXPOSURE_DOWN:
            self.config.exposure_time -= 100
            if self.config.exposure_time < 100:
                self.config.exposure_time = 100
            self.camera.set_exposure(self.config.exposure_time)
        # Help
        elif key == kb.HELP1 or key == kb.HELP2:
            self.show_help = ~self.show_help

        # Quit
        elif key == 27:
            self.scanner.is_scanning = False
            self.scanner.is_multi_scanning = False
            self.interrupt_flag = True
            self.config.save()
            self.quit_requested = True

    def menu_commands(self, key):
        kb = self.keyboard
        # Scan
        if key == kb.SCAN:
            print("begin scanning")
            self.scanner.is_multi_scanning = True
            self.scanner.multi_scan()

        # Home
        elif key == kb.HOME:
            self.stage.move_home(self.config.home_offset)
            print("Stage: move home")
        elif key == kb.SET_HOME:
            self.config.home_offset = self.stage.position
            print("Stage: set current position to home offset")

        # elif key == kb.SAVE_TO_CFG1:
        #     self.config.save("config_1")
        # elif key == kb.SAVE_TO_CFG2:
        #     self.config.save("config_2")
        # elif key == kb.SAVE_TO_CFG3:
        #     self.config.save("config_3")
        #
        # elif key == kb.LOAD_CFG1:
        #     self.config.load("config_1")
        # elif key == kb.LOAD_CFG2:
        #     self.config.load("config_2")
        # elif key == kb.LOAD_CFG3:
        #     self.config.load("config_3")

        # Move stage
        elif key == kb.FORWARD:
            self.stage.move_y(1000)
            print("Stage: move y 1mm")
        elif key == kb.BACK:
            self.stage.move_y(-1000)
            print("Stage: move y -1mm")
        elif key == kb.LEFT:
            self.stage.move_x(-1000)
            print("Stage: move x -1mm")
        elif key == kb.RIGHT:
            self.stage.move_x(1000)
            print("Stage: move x 1mm")

        elif key == kb.X_FORWARD:
            self.stage.move_y(10000)
            print("Stage: move y 10mm")
        elif key == kb.X_BACK:
            self.stage.move_y(-10000)
            print("Stage: move y -10mm")
        elif key == kb.X_LEFT:
            self.stage.move_x(-10000)
            print("Stage: move x -10mm")
        elif key == kb.X_RIGHT:
            self.stage.move_x(10000)
            print("Stage: move 10mm")

        elif key == kb.UP:
            self.stage.move_z(20)
            print("Stage: move z 20um")
        elif key == kb.DOWN:
            self.stage.move_z(-20)
            print("Stage: move z -20um")
        elif key == kb.X_UP:
            self.stage.move_z(200)
            print("Stage: move z 200um")
        elif key == kb.X_DOWN:
            self.stage.move_z(-200)
            print("Stage: move z -200um")

        elif key == ord('r'):
            self.stage.poll()
            print("Stage: poll position")

            # Scan scans edition
        elif key == kb.PREV_SCAN:  # Select previous scan zone
            if self.selected_scan_number > 1:
                self.selected_scan_number -= 1
        elif key == kb.NEXT_SCAN:  # Select next scan zone
            if self.selected_scan_number < len(self.scans):
                self.selected_scan_number += 1
        elif key == kb.ADD_ZONE:  # add a new zone
            self.scans.append({'FL': [10000, 50000, 2000],
                               'BR': [11000, 51000, 2000],
                               'BL_Z': 2000,
                               'Z_corrections': [0, 0]})
        
        elif key == kb.DEL_ZONE:  # delete currently selected zone
            if len(self.scans) > 1:
                if self.selected_scan_number == len(self.scans):
                    self.scans.pop(self.selected_scan_number - 1)
                    self.selected_scan_number -= 1
                else:
                    self.scans.pop(self.selected_scan_number - 1)
        elif key == kb.DEL_ALL_ZONES:  # delete all scans
            self.selected_scan_number = 1
            self.scans = [{'FL': [10000, 50000, 2000],
                           'BR': [11000, 51000, 2000],
                           'BL_Z': 2000,
                           'Z_corrections': [0, 0]}]
        elif key == kb.SCAN_FL:
            self.selected_scan()['FL'] = [self.stage.x, self.stage.y, self.stage.z]
            self.config.update_z_correction_terms(self.selected_scan_number - 1)
            self.config.save()
        elif key == kb.SCAN_BR:
            self.selected_scan()['BR'] = [self.stage.x, self.stage.y, self.stage.z]
            self.config.update_z_correction_terms(self.selected_scan_number - 1)
            self.config.save()
       
        elif key == kb.SET_Z_COR:
            self.config.update_z_correction_terms(self.selected_scan_number - 1, self.stage.z)
            self.config.save()

        # Move to scan area
        elif key == kb.MOVE_SCAN_FL:
            self.stage.goto(self.selected_scan()['FL'])
        elif key == kb.MOVE_SCAN_BR:
            self.stage.goto(self.selected_scan()['BR'])
        elif key == kb.MOVE_SCAN_BL:
            fl = self.selected_scan()['FL']
            br = self.selected_scan()['BR']
            self.stage.goto([fl[0], br[1], fl[2]])
        elif key == kb.MOVE_SCAN_FR:
            fl = self.selected_scan()['FL']
            br = self.selected_scan()['BR']
            self.stage.goto([br[0], fl[1], br[2]])

        # One-off take stack
        elif key == kb.TAKE_STACK1 or key == kb.TAKE_STACK2:
            self.take_stack_requested = True

        # Find floor
        elif key == ord('c'):
            self.scanner.find_floor()

    def scanning_commands(self, key):
        if key == self.keyboard.SCAN:
            self.scanner.is_scanning = False
            self.scanner.is_multi_scanning = False
            self.interrupt_flag = True

    def check_for_command(self, wait_time=20):
        # chrono = - time.perf_counter_ns()
        key = cv2.waitKey(wait_time)
        # chrono += time.perf_counter_ns()
        # print(chrono)
        
        if key == -1:
            return

        print(key)

        self.permanent_commands(key)
        if self.scanner.is_multi_scanning:
            self.scanning_commands(key)
        else:
            self.menu_commands(key)

    def display(self, im: np.array):
        kb = self.keyboard
        # Reduce size of image
        im = im[::4, ::4, :].astype(np.uint8)

        if self.img_mode > 1:
            # Image mode and 2, 3, 4 are red, green and blue.
            # If one of these are selected, create a new colour image just from that channel
            im = np.repeat(im[:, :, self.img_mode - 2][..., np.newaxis], 3, axis=-1)

        # Add some black space to left of image to draw current status
        LEFT_EDGE_SIZE = 300
        im = np.pad(im, [[0, 0], [LEFT_EDGE_SIZE, 0], [0, 0]])

        sel_scan_num = self.selected_scan_number
        sel_scan = self.selected_scan()
        blz = self.selected_scan()['BL_Z']
        
        text_status = []
        text_button = []
        text_help = []

        # Define the UI text to be displayed

        if self.scanner.is_multi_scanning:
            text_button = [
                "",
                "",
                "",
                "",
                "",
                "g t",
                "",
                "f",
                "[ ]",
                "{ }",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "p",
                "",
                "",
                "",
                "",
                "",
                "",
                "esc"
            ]
            
            if self.lang == "en":
                scan_command = "Stop scanning"
                direction = "top down" if self.config.top_down else "bottom up"
                text_status = [
                    "POSITION",
                    f"[X, Y, Z]: {[self.stage.x, self.stage.y, self.stage.z]}",
                    f"Home: {self.config.home_offset}",
                    "- - - - - - - - - - - -",
                    "CAMERA",
                    f"Exposure: {self.config.exposure_time}us",
                    "- - - - - - - - - - - -",
                    "STACK: " + direction
                    + f" | {self.scanner.current_pic_count}/{self.scanner.total_pic_count} pics",
                    f"Height: {self.config.stack_height}um",
                    f"Step: {self.config.stack_step}um",
                    "- - - - - - - - - - - -",
                    f"SCAN: {sel_scan_num}/{len(self.scans)}",
                    f"FL: {sel_scan['FL']}",
                    f"BR: {sel_scan['BR']}",
                    f"BL: Z={blz}",
                    "- - - - - - - - - - - -",
                    "COMMANDS",
                    f"{scan_command}",
                    "",
                    "",
                    "",
                    "",
                    "",
                    "",
                    "quit"
                ]
            if self.lang == "fr":
                scan_command = "Arreter le scan"
                direction = "de haut en bas" if self.config.top_down else "de bas en haut"
                text_status = [
                    "POSITION",
                    f"[X, Y, Z]: {[self.stage.x, self.stage.y, self.stage.z]}",
                    f"Origine: {self.config.home_offset}",
                    "- - - - - - - - - - - -",
                    "CAMERA",
                    f"Exposure: {self.config.exposure_time}us",
                    "- - - - - - - - - - - -",
                    "PILE: " + direction + f" | {self.scanner.current_pic_count}/{self.scanner.total_pic_count} pictures taken",
                    f"Hauteur: {self.config.stack_height}um",
                    f"Etape: {self.config.stack_step}um",
                    "- - - - - - - - - - - -",
                    f"SCAN: {sel_scan_num}/{len(self.scans)}",
                    f"AvGch: {sel_scan['FL']}",
                    f"ArDt: {sel_scan['BR']}",
                    f"ArGch: Z={blz}",
                    "- - - - - - - - - - - -",
                    "DEMANDES",
                    f"{scan_command}",
                    "",
                    "",
                    "",
                    "",
                    "",
                    "",
                    "quitter"
                ]
        else:
            text_button = [
                f"{chr(kb.FORWARD)} {chr(kb.BACK)} {chr(kb.LEFT)}",
                f"{chr(kb.RIGHT)} {chr(kb.UP)} {chr(kb.DOWN)}",
                "h",
                "",
                "",
                "g t",
                "",
                "f",
                "[ ]",
                "{ }",
                "",
                f"{chr(kb.PREV_SCAN)} {chr(kb.NEXT_SCAN)}",
                "i",
                "j",
                "u",
                "",
                "",
                "p",
                "v",
                "B",
                "N",
                "",
                "",
                "esc"
            ]
            
            if self.lang == "en":
                scan_command = "Start Scanning"
                direction = "top down" if self.config.top_down else "bottom up"
                text_status = [
                    "POSITION",
                    f"[X, Y, Z]: {[self.stage.x, self.stage.y, self.stage.z]}",
                    f"Home: {self.config.home_offset}",
                    "- - - - - - - - - - - -",
                    "CAMERA",
                    f"Exposure: {self.config.exposure_time}us",
                    "- - - - - - - - - - - -",
                    "STACK: " + direction,
                    f"Height: {self.config.stack_height}um",
                    f"Step: {self.config.stack_step}um",
                    "- - - - - - - - - - - -",
                    "SCAN" + self.config.top_down * " (precision +)",
                    f"Zone: {sel_scan_num}/{len(self.scans)}",
                    f"FL: {sel_scan['FL']}",
                    f"BR: {sel_scan['BR']}",
                    f"BL: Z={blz}",
                    "- - - - - - - - - - - -",
                    "COMMANDS",
                    f"{scan_command}",
                    "Add new zone",
                    "Delete current zone",
                    "Delete all scans",
                    "",
                    "",
                    "quit"
                ]
            if self.lang == "fr":
                scan_command = "Demarrer le scan"
                direction = "de haut en bas" if self.config.top_down else "de bas en haut"
                text_status = [
                    "POSITION",
                    f"[X, Y, Z]: {[self.stage.x, self.stage.y, self.stage.z]}",
                    f"Home: {self.config.home_offset}",
                    "- - - - - - - - - - - -",
                    "CAMERA",
                    f"Exposure: {self.config.exposure_time}us",
                    "- - - - - - - - - - - -",
                    "PILE: " + direction,
                    f"Hauteur: {self.config.stack_height}um",
                    f"Etape: {self.config.stack_step}um",
                    "- - - - - - - - - - - -",
                    f"SCAN: {sel_scan_num}/{len(self.scans)}",
                    f"AvGch: {sel_scan['FL']}",
                    f"ArDt: {sel_scan['BR']}",
                    f"ArGch: Z={blz}",
                    "- - - - - - - - - - - -",
                    "DEMANDES",
                    f"{scan_command}",
                    "Ajouter une zone",
                    "Suppr. la zone",
                    "Suppr. toutes les scans",
                    "",
                    "",
                    "quitter"
                ]
        
        # Define the help text to be displayed
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
                    "p: start/stop scan",
                    "esc: quit",
                    "?: close help",
                ]
            if self.lang == "fr":
                text_help = [
                    "h/H: fixer/aller a la position d'origine",
                    "z,s,q,d,a,e: avant, arriere, gauche, droit, haut, bas",
                    "Z,S,Q,D,A,E: 10 x avant, arriere, gauche, droit, haut, bas"
                    "[ ]: -/+ hauteur de pile (100um)",
                    "{ }: -/+ etape de pile (20um)",
                    "j/J: fixer/aller a l'avant-gauche de la zone de scan",
                    "i/I: fixer/aller a l'arriere droit de la zone de scan",
                    "entree: faire une pile",
                    "p:  demarrer/arreter un scan",
                    "esc: quitter",
                    "?: se fermer",
                ]
        else:
            if self.lang == "en":
                text_help = ["?: show help"]
            if self.lang == "fr":
                text_help = ["?: afficher l'aide"]

        # Draw the UIs text
        for i, t in enumerate(text_status):
            cv2.putText(im, t, (50, i * 20 + 20), cv2.FONT_HERSHEY_SIMPLEX, 0.4, (230, 230, 3055), 1, cv2.LINE_AA)

        for i, t in enumerate(text_button):
            cv2.putText(im, t, (10, i * 20 + 20), cv2.FONT_HERSHEY_SIMPLEX, 0.4, (120, 255, 255), 1, cv2.LINE_AA)

        for i, t in enumerate(text_help):
            cv2.putText(im, t, (LEFT_EDGE_SIZE + 10, i * 20 + 20), cv2.FONT_HERSHEY_SIMPLEX, 0.4, (0, 255, 255), 1,
                        cv2.LINE_AA)

        # Show the image in the UI using open CV
        cv2.imshow("im", im)

    def start(self):
        self.stage.start()
        self.camera.start()
        self.camera.set_exposure(self.config.exposure_time)
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
        return self.interrupt_flag
