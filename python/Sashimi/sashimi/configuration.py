import json
import os
import sashimi


class Configuration(object):
    def __init__(self):
        self.printer_ip = "http://172.20.211.175"
        self.port = "7125"
        self.home_position = [50000, 50000, 2000]
        self.stack_height = 1000
        self.stack_step = 60
        self.exposure_time = 2000
        self.z_margin = 200
        self.scans = [{'FL': [10000, 50000, 2000],
                       'BR': [11000, 51000, 2000],
                       'BL_Z': 2000,
                       'Z_corrections':[0, 0]}]
        self.camera_settings_dir = os.path.join(os.path.expanduser("~"), ".Sashimi", "CameraSettings")
        self.camera_settings_file = "nodeFile.pfs"

    def update_z_correction_terms(self, index, blz=None):
        # supposes the scan surface is flat and non-vertical
        fl, br = self.scans[index]['FL'], self.scans[index]['BR']
        x, y, z = 0, 1, 2

        if br[x] == fl[x] or br[y] == fl[y]:
            print("brx == flx or bry == fly !!!")
            return

        if blz is None:
            blz = (fl[z] + br[z])//2

        dz_dx = (blz - fl[z]) / (br[x] - fl[x])
        dz_dy = (br[z] - blz) / (br[y] - fl[y])

        self.scans[index]['BL_Z'] = blz
        self.scans[index]['Z_corrections'] = [dz_dx, dz_dy]
        self.save()

    def save(self, save_name="config"):
        config_file = os.path.join(os.path.expanduser("~"), ".Sashimi", save_name + ".json")
        os.makedirs(os.path.dirname(config_file), exist_ok=True)
        with open(config_file, "w") as f:
            j = json.dumps(self, default=lambda o: o.__dict__, sort_keys=True, indent=4)
            f.write(j)

    @staticmethod
    def load(save_name="config"):
        print("\n=========================================")
        print("          Loading configuration          \n")
        config_file = os.path.join(os.path.expanduser("~"), ".Sashimi", save_name + ".json")
        if os.path.exists(config_file):
            print(f"Configuration file found {config_file}\n")
            try:
                with open(config_file, "r") as f:
                    j = json.load(f)
                    config = Configuration()
                    for key, val in j.items():
                        if key in config.__dict__:
                            config.__dict__[key] = val
            except RuntimeError:
                config = Configuration()
        else:
            print("Configuration file not found, loading base configuration")
            config = Configuration()
        print("Configuration used:\n-------------------")
        for key, val in config.__dict__.items():
            print(f"{key}: {val}")
        return config
