import json
import os


class Configuration(object):
    def __init__(self):
        self.port = "COM3"
        self.home_offset = [10000, 50000, 2000]
        self.scan_front_left = [10000, 50000, 2000]
        self.scan_back_right = [60000, 125000, 2000]
        self.stack_height = 1000
        self.stack_step = 60
        self.exposure_time = 3000

    def save(self):
        config_file = os.path.join(os.path.expanduser("~"), ".sashimi", "config.json")
        os.makedirs(os.path.dirname(config_file), exist_ok=True)
        with open(config_file, "w") as f:
            j = json.dumps(self, default=lambda o: o.__dict__, sort_keys=True, indent=4)
            f.write(j)

    @staticmethod
    def load():
        print("Load config")
        config_file = os.path.join(os.path.expanduser("~"), ".sashimi", "config.json")
        if os.path.exists(config_file):
            print("Config file found")
            try:
                with open(config_file, "r") as f:
                    j = json.load(f)
                    print(j)
                    config = Configuration();
                    print(config.__dict__)
                    for key, val in j.items():
                        print(f"{key}: {val}")
                        if key in config.__dict__:
                            config.__dict__[key] = val
            except:
                config = Configuration();
        else:
            config = Configuration();
        return config

