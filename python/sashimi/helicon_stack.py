import argparse
import os
from glob import glob
import subprocess


def stack(dir):
    dirs = sorted([d for d in glob(os.path.join(dir, "*")) if os.path.isdir(d)])
    for d in dirs:
        print(d)
        fns = sorted(glob(os.path.join(d, "*.jpg")))
        if len(fns) == 0:
            continue
        command = [r"C:\\Program Files\\Helicon Software\\Helicon Focus 7\\HeliconFocus.exe",
                   "-silent",
                   f"{d}",
                   "-dmap"]
        print(command)
        subprocess.run(command)