import os
import sys
import time
from pathlib import Path
from glob import glob
from PIL import Image
import subprocess
from time import sleep


def get_helicon_focus():
    helicon_focus = r"C:\\Program Files\\Helicon Software\\Helicon Focus 7\\HeliconFocus.exe"
    if not os.path.exists(helicon_focus):
        helicon_focus = r"C:\Program Files\Helicon Software\Helicon Focus 8\HeliconFocus.exe"
        if not os.path.exists(helicon_focus):
            raise ResourceWarning("Helicon Focus was not found")
    return helicon_focus


def stack(_dir):
    helicon_focus = get_helicon_focus()

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
    helicon_focus = get_helicon_focus()
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


def stack_from_to(stacks_dir, save_dir):
    """
    :param stacks_dir:
    :param save_dir: (must include sub folder if multiple scans)
    """
    helicon_focus = get_helicon_focus()

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


def stack_for_multiple_exp(scan_path: Path, f_stacks_path: Path, exp_values: list):
    helicon_focus = get_helicon_focus()

    scan_name = scan_path.stem
    for exp in exp_values:
        output_folder = f_stacks_path.joinpath(f"E{exp}", scan_name)
        os.makedirs(output_folder, exist_ok=True)

        xy_folders = sorted([d for d in glob(os.path.join(scan_path, "*")) if os.path.isdir(d)])
        for folder in xy_folders:
            stack_name = Path(folder).stem
            stacked_file_path = output_folder.joinpath(stack_name)
            dd = Path(folder).joinpath(f"E{exp}")
            images = sorted(glob(os.path.join(dd, "*.jpg")))
            if len(images) == 0:
                continue

            command = [
                helicon_focus,
                "-silent",
                f"{dd}",
                "-mp:0",
                "-rp:4",
                f"-save:{stacked_file_path}.jpg"
            ]
            print(command)
            subprocess.run(command, shell=True)
            
            
def parallel_stack(queue, error_logs=None):
    if error_logs is not None:
        sys.stdout = sys.stderr = open(error_logs, mode='w', encoding='UTF-8')

    while True:
        if queue.empty():
            sleep(0.2)
            continue

        msg = queue.get()
        print(msg)
        if msg == "terminate":
            break
        else:
            xy_folder, output_folder = msg
            single_stack(Path(xy_folder), Path(output_folder))


def single_stack(xy_folder: Path, f_stacks: Path):
    scan_name = xy_folder.parent.stem
    f_stack = gen_stack(xy_folder, f_stacks.joinpath(scan_name))
    fs = Image.open(f_stack)
    fs.load()
    fs.save(f_stack.parent.joinpath(f"{f_stack.stem}.png"))
    os.remove(f_stack)

    # else:
    #     for exp in exposures:
    #         f_stacks_exp = f_stacks.parent.joinpath(f"E{exp}", f_stacks.stem)
    #         raw_stack = xy_folder.joinpath(f"E{exp}")
    #
    #         f_stack = gen_stack(raw_stack, f_stacks_exp)
    #         fs = Image.open(f_stack)
    #         fs.load()
    #         fs.save(f_stack.parent.joinpath(f"{f_stack.stem}.png"))
    #         os.remove(f_stack)


def gen_stack(img_dir: Path, o_dir: Path):
    sp = o_dir.joinpath(f"{img_dir.stem}.tiff")
    command = [get_helicon_focus(), "-silent", f"{img_dir}", "-mp:0", "-rp:4", f"-save:{sp}"]
    subprocess.run(command)
    return sp
