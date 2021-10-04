import cv2
import serial


class Stage(object):
    def __init__(self, port):
        self.port = port
        self.serial: serial.Serial = None

        self.x = 0
        self.y = 0
        self.z = 0

        self.reported_x = 0
        self.reported_y = 0
        self.reported_z = 0

        self.x_limits = (0, 200000)
        self.y_limits = (0, 200000)
        self.z_limits = (0, 10000)

        self.buffer = []

    def start(self):
        self.serial = serial.Serial(self.port, 115200)

    def stop(self):
        self.serial.close()

    @property
    def position(self):
        return [self.x, self.y, self.z]

    def move_home(self, offset):
        self.send_command("G28 R X Y Z")
        self.x = 0
        self.y = 0
        self.z = 0
        self.move_x(offset[0])
        self.move_y(offset[1])
        self.goto_z(offset[2] + 1000)
        self.goto_z(offset[2])
        self.wait_until_position(20000)

    def move_x(self, distance):
        self.goto_x(self.x + distance)

    def move_y(self, distance):
        self.goto_y(self.y + distance)

    def move_z(self, distance):
        self.goto_z(self.z + distance)

    def goto_x(self, position):
        self.x = position
        if self.x < self.x_limits[0]:
            self.x = self.x_limits[0]
        if self.x > self.x_limits[1]:
            self.x = self.x_limits[1]
        self.send_command(f"G1 X {self.x / 1000:3f} F3000")

    def goto_y(self, position):
        self.y = position
        if self.y < self.y_limits[0]:
            self.y = self.y_limits[0]
        if self.y > self.y_limits[1]:
            self.y = self.y_limits[1]
        self.send_command(f"G1 Y {self.y / 1000:3f} F3000")

    def goto_z(self, position):
        self.z = position
        if self.z < self.z_limits[0]:
            self.z = self.z_limits[0]
        if self.z > self.z_limits[1]:
            self.z = self.z_limits[1]
        self.send_command(f"G1 Z {self.z / 1000:3f} F100")

    def goto(self, position):
        self.goto_x(position[0])
        self.goto_y(position[1])
        self.goto_z(position[2])

    def poll(self):
        dummy = self.read()
        self.send_command("M114")
        cv2.waitKey(100)
        responses = self.read()
        for response in responses:
            if response.startswith("X:"):
                parts = response.split(' ')
                subparts = parts[0].split(':')
                self.reported_x = int(float(subparts[1])*1000)
                subparts = parts[1].split(':')
                self.reported_y = int(float(subparts[1]) * 1000)
                subparts = parts[2].split(':')
                self.reported_z = int(float(subparts[1]) * 1000)

    def wait_until_position(self, ms):
        i = 0
        while i < ms / 100:
            self.poll()
            if self.x == self.reported_x and self.y == self.reported_y and self.z == self.reported_z:
                print(f"Position attained in {i*100}ms")
                return
            i += 1
        print(f"!!! ERROR: position not attained after {ms}ms ")

    def send_command(self, command):
        self.serial.write((command + "\n").encode())
        # print((command + "\n").encode())

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
        # print(lines)
        return lines
