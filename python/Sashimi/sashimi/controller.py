from pathlib import Path
import cv2
import numpy as np
from sashimi.camera import Camera
from sashimi.scanner import Scanner
from sashimi.stage import Stage
from sashimi.configuration import Configuration
from sashimi.utils import Keyboard
from sashimi.user_interface import UserInterface
class Controller(object):
    def __init__(
            self,
            save_dir: str | Path,
            lang: str = "en",
            layout: str = 'AZERTY',
            z_margin: int = None,
            multi_exp: list[float] = None,
            remove_raw: bool = False,
            auto_f_stack: bool = True,
            lowest_z: bool = False,
            do_overwrite: bool = False):

        # saved/default config
        self.config = Configuration.load()

        # user parameters
        if type(save_dir) == str:
            save_dir = Path(save_dir)
        if z_margin is not None:
            self.config.z_margin = z_margin
            self.config.save()

        self.save_dir = save_dir
        self.lang = lang
        self.layout = layout
        self.multi_exp = multi_exp
        self.remove_raw = remove_raw
        self.auto_f_stack = auto_f_stack
        self.lowest_z = lowest_z
        self.do_overwrite = do_overwrite

        # parameters an variables
        self.img_mode = 1
        self.refresh_rate_Hz = 20
        self.frame_duration_ms = 1000 // self.refresh_rate_Hz
        self.selected_scan_number = 1
        self.interrupt_flag = False
        self.quit_requested = False
        self.show_help = False
        self.take_stack_requested = False
        self.start_scan_requested = False
        self.stop_scan_requested = False
        self.time_remaining = None

        # instances
        self.stage = Stage(self, self.config.printer_ip, self.config.port)
        self.camera = Camera(self, self.config.camera_settings_dir, self.config.camera_settings_file)
        self.scanner = Scanner(self)
        self.keyboard = Keyboard(self.layout)
        self.ui = UserInterface(self)

    def selected_scan(self):
        return self.config.scans[self.selected_scan_number - 1]

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

        # Exposure
        elif key == kb.EXPOSURE_UP:
            self.config.exposure_time += 50
            if self.config.exposure_time > 50000:
                self.config.exposure_time = 50000
            self.camera.set_exposure(self.config.exposure_time)
        elif key == kb.EXPOSURE_DOWN:
            self.config.exposure_time -= 50
            if self.config.exposure_time < 100:
                self.config.exposure_time = 100
            self.camera.set_exposure(self.config.exposure_time)
        # Help
        elif key == kb.HELP1 or key == kb.HELP2:
            self.show_help = ~self.show_help

        # Quit
        elif key == 27:
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
            self.stage.move_home(self.config.home_position)
            print("Stage: move home")
        elif key == kb.SET_HOME:
            self.config.home_position = self.stage.position
            print("Stage: set current position to home position")

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
            if self.selected_scan_number < len(self.config.scans):
                self.selected_scan_number += 1
        elif key == kb.ADD_ZONE:  # add a new zone
            self.config.scans.append({'FL': [10000, 50000, 2000],
                                      'BR': [11000, 51000, 2000],
                                      'BL_Z': 2000,
                                      'Z_corrections': [0, 0]})
        
        elif key == kb.DEL_ZONE:  # delete currently selected zone
            if len(self.config.scans) > 1:
                if self.selected_scan_number == len(self.config.scans):
                    self.config.scans.pop(self.selected_scan_number - 1)
                    self.selected_scan_number -= 1
                else:
                    self.config.scans.pop(self.selected_scan_number - 1)
            self.config.save()
        elif key == kb.DEL_ALL_ZONES:  # delete all scans
            self.selected_scan_number = 1
            self.config.scans = [{'FL': [10000, 50000, 2000],
                                  'BR': [11000, 51000, 2000],
                                  'BL_Z': 2000,
                                  'Z_corrections': [0, 0]}]
            self.config.save()
        elif key == kb.SCAN_FL:
            scan = self.selected_scan()
            if self.stage.x == scan['BR'][0] or self.stage.y == scan['BR'][1]:
                return
            self.selected_scan()['FL'] = [self.stage.x, self.stage.y, self.stage.z]
            self.config.update_z_correction_terms(self.selected_scan_number - 1)
            self.config.save()
        elif key == kb.SCAN_BR:
            scan = self.selected_scan()
            if self.stage.x == scan['FL'][0] or self.stage.y == scan['FL'][1]:
                return
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
        elif key == ord('C'):
            self.scanner.find_floor()

    def scanning_commands(self, key):
        if key == self.keyboard.SCAN:
            self.scanner.is_multi_scanning = False
            self.interrupt_flag = True

    def check_for_command(self, wait_time=50):
        key = cv2.waitKey(wait_time)
        if key == -1:
            return
        # print(key)

        self.permanent_commands(key)
        if self.scanner.is_multi_scanning:
            self.scanning_commands(key)
        else:
            self.menu_commands(key)
        return True

    def start(self):
        self.camera.start()
        self.camera.set_exposure(self.config.exposure_time)
        print("\n=========================================")
        print("          Printer initialization         \n")
        self.stage.move_home(self.config.home_position)

        # Control loop
        while not self.quit_requested:
            # self.wait()
            img = self.camera.latest_image()
            if img is not None:
                self.ui.render(img)
            self.check_for_command(self.frame_duration_ms)

        # Clean up
        cv2.destroyAllWindows()
        self.camera.stop()
        return self.interrupt_flag
