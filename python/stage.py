import cv2
import serial


class Stage(object):
    def __init__(self, port):
        self.port = port
        self.serial: serial.Serial = None

        self.x = 0
        self.y = 0
        self.z = 0

        self.x_limits = (0, 200000)
        self.y_limits = (0, 200000)
        self.z_limits = (0, 10000)
        self.home_offset = (0, 30000, 1000)

        self.buffer = []

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
        self.goto_z(self.home_offset[2] + 1000)
        self.goto_z(self.home_offset[2])

    def move_x(self, distance):
        self.x += distance
        if self.x < self.x_limits[0]:
            self.x = self.x_limits[0]
        if self.x > self.x_limits[1]:
            self.x = self.x_limits[1]
        self.send_command(f"G1 X {self.x / 1000:3f} F3000")

    def move_y(self, distance):
        self.y += distance
        if self.y < self.y_limits[0]:
            self.y = self.y_limits[0]
        if self.y > self.y_limits[1]:
            self.y = self.y_limits[1]
        self.send_command(f"G1 Y {self.y / 1000:3f} F3000")

    def move_z(self, distance):
        self.z += distance
        if self.z < self.z_limits[0]:
            self.z = self.z_limits[0]
        if self.z > self.z_limits[1]:
            self.z = self.z_limits[1]
        self.send_command(f"G1 Z {self.z / 1000:3f} F100")

    def goto_z(self, position):
        self.z = position
        if self.z < self.z_limits[0]:
            self.z = self.z_limits[0]
        if self.z > self.z_limits[1]:
            self.z = self.z_limits[1]
        self.send_command(f"G1 Z {self.z / 1000:3f} F100")

    def poll(self):
        dummy = self.read()
        self.send_command("M114")
        cv2.waitKey(100)
        responses = self.read()
        for response in responses:
            if response.startswith("X:"):
                parts = response.split(' ')
                subparts = parts[0].split(':')
                self.x = int(float(subparts[1])*1000)
                subparts = parts[1].split(':')
                self.y = int(float(subparts[1]) * 1000)
                subparts = parts[2].split(':')
                self.z = int(float(subparts[1]) * 1000)

    def send_command(self, command):
        self.serial.write((command + "\n").encode())
        print((command + "\n").encode())

    def read(self):
        lines = []
        while self.serial.in_waiting > 0:
            for b in self.serial.read():
                if b != ord('\n') and b != ord('\r'):
                    self.buffer.append(chr(b))
                else:
                    line = ''.join(self.buffer)
                    print(line)
                    lines.append(line)
                    self.buffer = []
        print(lines)
        return lines
