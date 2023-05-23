
class Keyboard(object):
	def __init__(self, layout='QWERTY'):
		self.layout = layout
		self.HOME = ord('H')
		self.SET_HOME = ord('h')
		
		self.FORWARD = ord('w')
		self.BACK = ord('s')
		self.LEFT = ord('a')
		self.RIGHT = ord('d')
		self.UP = ord('q')
		self.DOWN = ord('e')
		
		self.X_FORWARD = ord('W')
		self.X_BACK = ord('S')
		self.X_LEFT = ord('A')
		self.X_RIGHT = ord('D')
		self.X_UP = ord('Q')
		self.X_DOWN = ord('E')
		
		self.EXPOSURE_UP = ord('t')
		self.EXPOSURE_DOWN = ord('g')
		
		self.SCAN_FL = ord('j')
		self.SCAN_BR = ord('i')
		self.SET_Z_COR = ord('u')
		
		self.MOVE_SCAN_FL = ord('J')
		self.MOVE_SCAN_BL = ord('U')
		self.MOVE_SCAN_BR = ord('I')
		self.MOVE_SCAN_FR = ord('K')
		self.SCAN = ord('p')
		
		self.HELP1 = ord('?')
		self.HELP2 = ord('/')
		
		self.PREV_SCAN = ord('z')
		self.NEXT_SCAN = ord('x')
		self.ADD_ZONE = ord('v')
		self.DEL_ZONE = ord('B')
		self.DEL_ALL_ZONES = ord('N')
		
		self.TAKE_STACK1 = ord('\n')
		self.TAKE_STACK2 = ord('\r')
		
		self.FLIP_STACK_ORDER = ord('f')
		
		self.SAVE_TO_CFG1 = ord('5')
		self.SAVE_TO_CFG2 = ord('6')
		self.SAVE_TO_CFG3 = ord('7')
		
		self.LOAD_CFG1 = ord('8')
		self.LOAD_CFG2 = ord('9')
		self.LOAD_CFG3 = ord('0')
		
		if self.layout == 'AZERTY':
			self.FORWARD = ord('z')
			self.BACK = ord('s')
			self.LEFT = ord('q')
			self.RIGHT = ord('d')
			self.UP = ord('a')
			self.DOWN = ord('e')
			
			self.X_FORWARD = ord('Z')
			self.X_BACK = ord('S')
			self.X_LEFT = ord('Q')
			self.X_RIGHT = ord('D')
			self.X_UP = ord('A')
			self.X_DOWN = ord('E')
			self.PREV_SCAN = ord('w')
