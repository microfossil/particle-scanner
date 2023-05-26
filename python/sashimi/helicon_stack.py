import os
from pathlib import Path
from glob import glob
import subprocess
import time


def check_for_helicon_focus():
    helicon_focus = r"C:\\Program Files\\Helicon Software\\Helicon Focus 7\\HeliconFocus.exe"
    if not os.path.exists(helicon_focus):
        helicon_focus = r"C:\Program Files\Helicon Software\Helicon Focus 8\HeliconFocus.exe"
        if not os.path.exists(helicon_focus):
            raise ResourceWarning("Helicon Focus was not found")
    return helicon_focus


def stack(_dir):
    helicon_focus = check_for_helicon_focus()

    dirs = sorted([d for d in glob(os.path.join(_dir, "*")) if os.path.isdir(d)])
    for d in dirs:
        print(d)
        fns = sorted(glob(os.path.join(d, "*.jpg")))
        if len(fns) == 0:
            continue
        command = [helicon_focus,
                   "-silent",
                   f"{d}",
                   "-dmap",
                   "-rp:4"]
        print(command)
        subprocess.run(command)


def radius_test(stacks_dir, save_dir):
    helicon_focus = check_for_helicon_focus()
    dirs = sorted([d for d in glob(os.path.join(stacks_dir, "*")) if os.path.isdir(d)])
    for radius in range(0, 11):
        for d in dirs:
            folder_name = f"radius_{radius}px"
            save_name = save_dir.joinpath(folder_name, Path(d).stem)
            images = sorted(glob(os.path.join(d, "*.jpg")))
            if len(images) == 0:
                continue
            command = [helicon_focus,
                       "-silent",
                       f"{d}",
                       "-mp:0",
                       f"-rp:{radius}",
                       f"-save:{save_name}.jpg"]
            print(command)
            subprocess.run(command)


def process_pictures(_queue):
    print("dans process_pictures")
    interrupted = False
    while not interrupted:
        if _queue.empty():
            print("sleep(1) commence")
            time.sleep(1)
            print("sleep(1) est terminé")
            continue

        msg = _queue.get()
        if msg == 'interrupt':
            interrupted = True
            continue
        elif msg is list:
            stack_from_to(msg[0], msg[1])
        else:
            print("message invalide :")
            print(msg)


def stack_from_to(stacks_dir, save_dir):
    """
    :param stacks_dir:
    :param save_dir: (must include sub folder if multiple scans)
    """
    helicon_focus = check_for_helicon_focus()

    dirs = sorted([d for d in glob(os.path.join(stacks_dir, "*")) if os.path.isdir(d)])
    for d in dirs:
        print(d)
        save_subdir = save_dir.joinpath(Path(d).stem)
        images = sorted(glob(os.path.join(d, "*.jpg")))
        if len(images) == 0:
            continue

        command = [helicon_focus,
                   "-silent",
                   f"{d}",
                   "-mp:0",
                   "-rp:4",
                   f"-save:{save_subdir}.jpg"]
        print(command)
        subprocess.run(command)


def stack_for_multiple_exp(_from: Path, _to: Path, exp_values: list):
    helicon_focus = check_for_helicon_focus()

    scan_name = _from.stem
    for exp in exp_values:
        fs_folder = _to.joinpath(f"E{exp}", scan_name)
        os.makedirs(fs_folder, exist_ok=True)

        dirs = sorted([d for d in glob(os.path.join(_from, "*")) if os.path.isdir(d)])
        for d in dirs:
            stack_name = Path(d).stem
            save_name = fs_folder.joinpath(stack_name)
            dd = Path(d).joinpath(f"E{exp}")
            images = sorted(glob(os.path.join(dd, "*.jpg")))
            if len(images) == 0:
                continue

            command = [
                helicon_focus,
                "-silent",
                f"{dd}",
                "-mp:0",
                "-rp:4",
                f"-save:{save_name}.jpg"
            ]
            print(command)
            subprocess.run(command, shell=True)






