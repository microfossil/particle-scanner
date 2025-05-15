from pypylon import pylon

"""
This file is not used in project for now. It is kept 
in case it is needed in the future for externalising the saving of the
camera config file done in camera.py (save_camera_settings function).

It can be used manually by running 'python get_cam_settings.py' 
in the shell to save this setting file manually though.
"""

def save_camera_settings(camera, node_file="nodeFile.pfs"):
    n_map = camera.GetNodeMap()
    pylon.FeaturePersistence.Save(node_file, n_map)

if __name__ == '__main__':
    camera = pylon.InstantCamera(pylon.TlFactory.GetInstance().CreateFirstDevice())
    camera.Open()
    save_camera_settings(camera)
    camera.Close()