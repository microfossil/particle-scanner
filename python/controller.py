import cv2
import numpy as np


from stage import Stage


class Controller(object):
    def __init__(self, scanner, stage: Stage, keyboard="en"):
        self.img_mode = 1
        self.scanner = scanner
        self.stage = stage
        self.quit_requested = False
        self.show_help = False
        self.take_stack_requested = False

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

            self.HELP1 = '?'
            self.HELP2 = '/'

            self.TAKE_STACK1 = '\n'
            self.TAKE_STACK2 = '\r'


    def check_for_command(self, wait_time=10):
        key = cv2.waitKey(wait_time)
        if key == -1:
            return
        print(key)

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

        # Stack control
        elif key == ord('{'):
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

        # Commands
        elif key == ord(self.TAKE_STACK1) or key == ord(self.TAKE_STACK2):
            self.take_stack_requested = True
        elif key == ord('c'):
            self.scanner.find_floor()

        # Help
        elif key == ord(self.HELP1) or key == ord(self.HELP2):
            self.show_help = ~self.show_help
            print("Controller: quit")

        # Quit
        elif key == 27:
            self.quit_requested = True
            print("Controller: quit")

    def display(self, im: np.array):
        im = im[::4, ::4, :].astype(np.uint8)
        if self.img_mode > 1:
            im = np.repeat(im[:, :, self.img_mode-2][..., np.newaxis], 3, axis=-1)
        text = []
        text.append(f"X: {self.stage.x}, Y: {self.stage.y}, Z: {self.stage.z}")
        text.append(f"H: {self.scanner.stack_height}, S: {self.scanner.stack_step}")

        if self.show_help:
            text.append("w,a,s,d,q,e: forward, left, back, right, up, down")
            text.append("W,A,S,D,Q,E: 10 x forward, left, back, right, up, down")
            text.append("[ ]: increase / decrease stack height (100um)")
            text.append("{ }: increase / decrease stack step (20um)")
            text.append("enter: take stack")
            text.append("escape: quit")
            text.append("?: close help")
        else:
            text.append("?: show help")
        for i, t in enumerate(text):
            cv2.putText(im, t, (10, i*20+20), cv2.FONT_HERSHEY_SIMPLEX, 0.4, (0, 255, 255), 1)

        return im