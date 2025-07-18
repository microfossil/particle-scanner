import json
import os

class Configuration(object):
    # Class variable for default config directory
    DEFAULT_CONFIG_DIR = os.path.join(os.path.expanduser("~"), ".Sashimi")

    def __init__(self):
        self.printer_ip = None
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
        self.config_dir = Configuration.DEFAULT_CONFIG_DIR
        self.camera_settings_file = "camera_settings.pfs"

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

    def save(self, config_dir=None, save_name="config"):
        if config_dir is None:
            config_dir = self.config_dir
        config_file_path = os.path.join(config_dir, save_name + ".json")

        os.makedirs(os.path.dirname(config_file_path), exist_ok=True)
        with open(config_file_path, "w") as f:
            j = json.dumps(self, default=lambda o: o.__dict__, sort_keys=True, indent=4)
            f.write(j)

    @staticmethod
    def load(config_dir=None, save_name="config"):
        if config_dir is None:
            config_dir = Configuration.DEFAULT_CONFIG_DIR
        config_file_path = os.path.join(config_dir, save_name + ".json")

        print("\n=========================================")
        print("          Loading configuration          \n")

        if os.path.exists(config_file_path):
            print(f"Configuration file found {config_file_path}\n")
            try:
                with open(config_file_path, "r") as f:
                    j = json.load(f)
                    config = Configuration()
                    for key, val in j.items():
                        if key in config.__dict__:
                            config.__dict__[key] = val
            except RuntimeError:
                print("Unable to load configuration file, loading base configuration")
                config = Configuration()
        else:
            print("Configuration file not found, loading base configuration")
            config = Configuration()

            # Demander à l'utilisateur d'entrer l'adresse IP manuellement
            print("\nFirst Particle-scanner usage detected!")
            print("Enter the IP address of your 3D printer:")
            print("(Format: http://192.168.1.100 or 172.20.211.175) for example\n")

            while True:
                user_ip = input("IP adress: ").strip()
                if user_ip:
                    # Vérifier si l'utilisateur a inclus http://
                    if not user_ip.startswith("http://") and not user_ip.startswith("https://"):
                        user_ip = "http://" + user_ip
                    config.printer_ip = user_ip
                    confirm = input(f"Confirm IP address '{user_ip}'? (y/n): ").strip()
                    if confirm != 'y':
                        print("IP address not confirmed. Please enter again.")
                        continue
                    config.save(save_name) # Save config with new IP
                    print("New IP validated")
                    print(f"Configuration file saved at {config_file_path}")
                    break
                else:
                    print("Enter a valid IP address.")

        print("Configuration used:\n-------------------")
        for key, val in config.__dict__.items():
            print(f"{key}: {val}")
        return config
