from sashimi.controller import Controller
from pathlib import Path
import os
import shutil

if __name__ == "__main__":
    my_dir = Path("C:/Users/utilisateur.SF_GEOL_STAG_IN/Desktop/Data/dump")
    os.makedirs(my_dir, exist_ok=True)

    for files in os.listdir(my_dir):
        path = os.path.join(my_dir, files)
        try:
            shutil.rmtree(path)
        except OSError:
            os.remove(path)

    controller = Controller(my_dir, "COM5", lang="en", layout='AZERTY', auto_f_stack=True, lowest_z=True)
    controller.start()
