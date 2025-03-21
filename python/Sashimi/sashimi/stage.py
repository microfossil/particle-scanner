import cv2
import serial
import time
import requests

class Stage(object):
    def __init__(self, controller, printer_ip, port):
        self.controller = controller
        self.printer_ip = printer_ip
        self.port = port
        # self.serial: serial.Serial = None

        # Distances in micrometers
        self.x = 0
        self.y = 0
        self.z = 0

        self.reported_x = 0
        self.reported_y = 0
        self.reported_z = 0

        self.x_limits = (0, 200000)
        self.y_limits = (0, 200000)
        self.z_limits = (0, 20000)

        self.buffer = []

    @property
    def position(self):
        return [self.x, self.y, self.z]

    def move_home(self, home_position):
        self.send_gcode("G28 R X Y Z")
        self.x = 0
        self.y = 0
        self.z = 0
        self.move_x(home_position[0])
        self.move_y(home_position[1])
        self.goto_z(home_position[2] + 1000)
        self.goto_z(home_position[2])
        # self.wait_until_position(20000)

    def move_x(self, distance_um):
        self.goto_x(self.x + distance_um)

    def move_y(self, distance_um):
        self.goto_y(self.y + distance_um)

    def move_z(self, distance_um):
        sleep_time = abs(distance_um) / 1000
        self.goto_z(self.z + distance_um)
        time.sleep(sleep_time)

    def goto_x(self, position):
        self.x = position
        if self.x < self.x_limits[0]:
            self.x = self.x_limits[0]
        if self.x > self.x_limits[1]:
            self.x = self.x_limits[1]
        self.send_gcode(f"G0 X {self.x / 1000:3f} F3000")

    def goto_y(self, position):
        self.y = position
        if self.y < self.y_limits[0]:
            self.y = self.y_limits[0]
        if self.y > self.y_limits[1]:
            self.y = self.y_limits[1]
        self.send_gcode(f"G0 Y {self.y / 1000:3f} F3000")

    def goto_z(self, position):
        self.z = position
        if self.z < self.z_limits[0]:
            self.z = self.z_limits[0]
        if self.z > self.z_limits[1]:
            self.z = self.z_limits[1]
        self.send_gcode(f"G0 Z {self.z / 1000:3f} F100")

    def goto(self, position):
        self.goto_x(position[0])
        self.goto_y(position[1])
        self.goto_z(position[2])

    def send_gcode(self, command, timeout=60):
        url = f"{self.printer_ip}:{self.port}/printer/gcode/script"
        payload = {"script": command}
        try:
            response = requests.post(url, json=payload, timeout=timeout)
            return response.json()
        except requests.Timeout as e:
            print(f"Request timed out after {timeout} seconds.\n{e}")
        except requests.RequestException as e:
            print(f"An error occurred: {e}")

    def get_query_printer_object(self, command, timeout=60):
        url = f"{self.printer_ip}:{self.port}/printer/objects/query"
        payload = {"objects": command}
        try:
            response = requests.post(url, json=payload, timeout=timeout)
            return response.json()
        except requests.Timeout as e:
            print(f"Request timed out after {timeout} seconds.\n{e}")
        except requests.RequestException as e:
            print(f"An error occurred: {e}")
