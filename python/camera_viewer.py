import cv2
from pypylon import pylon
import serial


class Stage(object):
    def __init__(self, port):
        self.port = port
        self.serial = None

        self.x = 0
        self.y = 0
        self.z = 0

        self.x_limits = (0, 200000)
        self.y_limits = (0, 200000)
        self.z_limits = (0, 10000)
        self.home_offset = (0, 20000, 1000)

    def start(self):
        self.serial = serial.Serial(self.port, 115200)

    def stop(self):
        self.serial.close()

    def move_home(self):
        self.send_command("G28 R X Y Z")
        self.x = 0
        self.y = 0
        self.z = 0
        self.move_x(self.home_offset[0])
        self.move_y(self.home_offset[1])
        self.move_z(self.home_offset[2])

    def move_x(self, distance):
        self.x += distance
        if self.x < self.x_limits[0]:
            self.x = self.x_limits[0]
        if self.x > self.x_limits[1]:
            self.x = self.x_limits[1]
        self.send_command(f"G1 X {self.x/1000:3f} F3000")

    def move_y(self, distance):
        self.y += distance
        if self.y < self.y_limits[0]:
            self.y = self.y_limits[0]
        if self.y > self.y_limits[1]:
            self.y = self.y_limits[1]
        self.send_command(f"G1 Y {self.y/1000:3f} F3000")
        
    def move_z(self, distance):
        self.z += distance
        if self.z < self.z_limits[0]:
            self.z = self.z_limits[0]
        if self.z > self.z_limits[1]:
            self.z = self.z_limits[1]
        self.send_command(f"G1 Z {self.z/1000:3f} F100")

    def send_command(self, command):
        self.serial.write((command + "\n").encode())
        print((command + "\n").encode())


class Controller(object):
    def __init__(self, stage: Stage):
        self.img_mode = 1
        self.stage = stage
        self.quit_requested = False

    def check_for_command(self):
        key = cv2.waitKey(10)
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
        # Quit
        elif key == 27:
            self.quit_requested = True
            print("Controller: quit")


# Open stage
stage = Stage('COM3')
stage.start()

# Open camera
camera = pylon.InstantCamera(pylon.TlFactory.GetInstance().CreateFirstDevice())
camera.Open()

# Create controller
controller = Controller(stage)

# Create format converter
converter = pylon.ImageFormatConverter()
converter.OutputPixelFormat = pylon.PixelType_BGR8packed

# Grab loop
camera.StartGrabbing()
while camera.IsGrabbing():
    grabResult = camera.RetrieveResult(5000, pylon.TimeoutHandling_ThrowException)
    if grabResult.GrabSucceeded():
        img = converter.Convert(grabResult).Array

        # Display the image
        if controller.img_mode == 1:
            cv2.imshow("im", img[::4, ::4, :])
        else:
            cv2.imshow("im", img[::4, ::4, controller.img_mode - 2])

        # Process any key commands
        controller.check_for_command()

        # Check if quit
        if controller.quit_requested:
            grabResult.Release()
            break

    grabResult.Release()

# Clean up
cv2.destroyAllWindows()
stage.stop()
camera.StopGrabbing()
camera.Close()
