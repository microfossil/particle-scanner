import os

import sashimi.helicon_stack as hs
import multiprocessing as mp
import datetime as dt
from pathlib import Path


def invent_default_directory():
	d = dt.datetime.now(tz=dt.timezone(dt.timedelta(hours=2)))
	o_name = f"{d.day}{d.month}{d.year}_{d.hour}{d.minute}{d.second}_zstacks"
	return Path("~").expanduser().joinpath("obj_det", o_name)


if __name__ == "__main__":
	input_dir = Path(input("input directory: "))
	output_dir = Path(input("output directory: "))
	
	if output_dir is None:
		output_dir = invent_default_directory()
		os.makedirs(output_dir)
		print(f'No output dir was given. Output will be saved on :\n{output_dir}')
	
	error_logs = input_dir.joinpath('error_logs.txt')
	queue = mp.Queue()
	stacking_process = mp.Process(target=hs.parallel_stack, args=(queue, None, error_logs))
	
	scan_directories = [d for d in input_dir.iterdir() if 'scan' in d.name and d.is_dir()]
	for scan_dir in scan_directories:
		queue.put((scan_dir, output_dir))
