import threading
import cv2
from pypylon import pylon
from controller import Controller


capture_lock = threading.Lock()

class CaptureThread(threading.Thread):
    def __init__(self,
                 camera: pylon.InstantCamera,
                 converter: pylon.ImageFormatConverter,
                 controller: Controller,
                 group=None,
                 target=None,
                 name=None,
                 args=(),
                 kwargs=None,
                 verbose=None):
        super(CaptureThread, self).__init__()
        self.camera = camera
        self.converter = converter
        self.controller = controller
        self.target = target
        self.name = name
        self.image = None

    def run(self):
        self.camera.StartGrabbing()
        while self.camera.IsGrabbing():
            grabResult = self.camera.RetrieveResult(5000, pylon.TimeoutHandling_ThrowException)
            if grabResult.GrabSucceeded():
                with capture_lock:
                    self.image = self.converter.Convert(grabResult).Array
                # Check if quit
                if self.controller.quit_requested:
                    grabResult.Release()
                    break
            grabResult.Release()
        return


class Camera(object):
    def __init__(self, controller: Controller):
        self.image = None
        self.camera = None
        self.controller = controller
        self.converter = pylon.ImageFormatConverter()
        self.converter.OutputPixelFormat = pylon.PixelType_BGR8packed
        self.capture_thread = None

    def start(self):
        # Open camera
        self.camera = pylon.InstantCamera(pylon.TlFactory.GetInstance().CreateFirstDevice())
        self.camera.Open()
        self.capture_thread = CaptureThread(self.camera, self.converter, self.controller)
        self.capture_thread.start()

    def stop(self):
        self.camera.StopGrabbing()
        self.camera.Close()

    def latest_image(self):
        img = None
        with capture_lock:
            if self.capture_thread.image is not None:
                img = self.capture_thread.image.copy()
        return img

