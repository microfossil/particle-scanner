from sashimi.helicon_stack import stack_for_multiple_exp
from pathlib import Path

from_ = Path(input("path from: "))
to_ = Path(input("Path to: "))
stack_for_multiple_exp(from_, to_, [1400])

