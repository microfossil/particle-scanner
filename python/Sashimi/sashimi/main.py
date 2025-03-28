import os
from sashimi.controller import Controller
from sashimi.utils import make_unique_subdir


if __name__ == "__main__":
    sashimi_output_dir = os.path.join(os.path.expanduser("~"), ".Sashimi", "output")
    os.makedirs(os.path.dirname(sashimi_output_dir), exist_ok=True)
    my_dir = make_unique_subdir(sashimi_output_dir)
    controller = Controller(my_dir, "COM3", lang="en", layout='AZERTY', auto_f_stack=True, lowest_z=True)
    controller.start()
