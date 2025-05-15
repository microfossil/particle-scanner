from enum import Enum

class State(Enum):
    IDLE = 'idle'
    INIT = 'initializing'
    HOME = 'homing'
    AUTO_LEVEL = 'auto_leveling'
    SCAN = 'scanning'
    INTERRUPT = 'interrupting'
    QUIT = 'quitting'
