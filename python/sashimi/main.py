from sashimi.controller import Controller
import os
import shutil


my_dir = "C:/Users/utilisateur.SF_GEOL_STAG_IN/Desktop/Data/X/dump"

for files in os.listdir(my_dir):
    path = os.path.join(my_dir, files)
    try:
        shutil.rmtree(path)
    except OSError:
        os.remove(path)

controller = Controller(my_dir, "COM5", lang="fr")
controller.start()
