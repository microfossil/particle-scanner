import cv2
import serial
import time
import requests
from decimal import Decimal, getcontext

# Set precision for correction factor computation
getcontext().prec = 10

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
        response = self.send_gcode("G28")
        if response["result"] == "ok":
            print('Printer homed successfully')

        self.goto_x(home_position[0])
        self.goto_y(home_position[1])
        self.goto_z(home_position[2])

        if self.check_position_reached(home_position[0], home_position[1], home_position[2]):
            print("Printer reached position X:" \
                  f"{home_position[0]}µm Y:{home_position[1]}µm Z:{home_position[2]}µm")

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

    def correction_factor_compute(self):
        """
        There is currently an offset between Z position sent to the 3D printer 
        and Z position returned from it. 
            e.g. When Requesting printer to get to Z pos. at 2mm
              field "position" from printer object "gcode_move" will return 1.9944mm
        This offset is linearly increasing with absolute Z position
            i.e. When Requesting printer to get to Z at 1mm, offset will be 1 x 0.0028mm, requesting
                 it to get to Z at 2mm, offset will be 2 x 0.0028mm, etc.
        This offset might be due to the precision and accuracy limitations of the printer's hardware
        and firmware. 
        We compensate this difference by introducing correction_factor variable. It is computed 
        as the difference between the Z position the printer is moved to (1mm) and the position read 
        from it.
        """
        self.send_gcode("G0 Z 1")
        response = self.get_query_printer_object({"gcode_move": ["position"]})
        z_position_read = response['result']['status']['gcode_move']['position'][2]
        correction_factor = Decimal('1') - Decimal(f'{z_position_read}')
        print(f"Z correction factor = {correction_factor}")
        return correction_factor

    def check_position_reached(self, x_target, y_target, z_target):
        """This function checks if the printer has reached the target position"""
        response = self.get_query_printer_object({"gcode_move": ["position"]})
        x_position_read = response['result']['status']['gcode_move']['position'][0]
        y_position_read = response['result']['status']['gcode_move']['position'][1]
        z_position_read = response['result']['status']['gcode_move']['position'][2]

        correction_factor = self.correction_factor_compute()
        offset = float(correction_factor * z_target / 1000)

        if (
            x_position_read == x_target/1000 and
            y_position_read == y_target/1000 and
            z_position_read + offset == z_target/1000
            ):
            return True
        
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
