from sashimi.helicon_stack import radius_test
from pathlib import Path

stacks_path = input("stacks path")

radius_test(Path(stacks_path), Path(stacks_path).joinpath("stacks"))
