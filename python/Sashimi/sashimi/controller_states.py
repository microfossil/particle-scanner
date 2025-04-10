from enum import Enum

class State(Enum):
    IDLE = 'idle'
    INIT = 'initializing...'
    HOMING = 'homing...'
    AUTO_LEVELING = 'auto_leveling...'
    SCANNING = 'scanning...'