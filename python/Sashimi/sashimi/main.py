from sashimi.controller import Controller
from pathlib import Path
from sashimi.util import make_unique_subdir
import os
import shutil

if __name__ == "__main__":
    my_dir = make_unique_subdir()
    controller = Controller(my_dir, "COM5", lang="en", layout='AZERTY', auto_f_stack=True, lowest_z=True)
    controller.start()
