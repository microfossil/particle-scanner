from time import perf_counter


class Timer(object):
	"""
	A simple timer object that can make laps, and give an average lap duration,
	although it only memorizes the duration of the last lap and the total time.
	"""
	def __init__(self):
		self.accu_time = 0
		self.lap_accu_time = 0
		self.start_date = None
		self.prev_lap_duration = None
		self.last_lap_date = None
		self.is_running = False
		self.lap_count = 0
		self.average_lap_time = None
	
	def start(self):
		self.is_running = True
		self.start_date = perf_counter()
		self.last_lap_date = self.start_date
		if self.lap_count == 0:
			self.lap_count += 1
		
	def stop(self):
		stop_date = perf_counter()
		self.accu_time += stop_date - self.start_date
		self.lap_accu_time += stop_date - self.last_lap_date
		self.is_running = False
		
	def duration(self):
		if self.is_running:
			return perf_counter() - self.start_date + self.accu_time
		else:
			return self.accu_time
	
	def lap_duration(self):
		if self.is_running:
			return perf_counter() - self.start_date + self.lap_accu_time
		else:
			return self.lap_accu_time
	
	def new_lap(self):
		if not self.is_running:
			raise RuntimeError("object Timer is not running, a new lap cannot be created.")
		self.lap_count += 1
		new_lap_date = perf_counter()
		self.prev_lap_duration = new_lap_date - self.last_lap_date + self.lap_accu_time
		self.average_lap_time = (new_lap_date - self.start_date + self.accu_time) / self.lap_count
		self.last_lap_date = new_lap_date
		self.lap_accu_time = 0
		return self.last_lap_date
