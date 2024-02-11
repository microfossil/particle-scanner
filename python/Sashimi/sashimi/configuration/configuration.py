import json
import os
from dataclasses import dataclass, field
from pathlib import Path
from typing import Optional

from sashimi.configuration.base import BaseModel
from sashimi.hardware.camera import CameraConfiguration
from sashimi.hardware.scanner import ScannerConfiguration
from sashimi.hardware.stage import StageConfiguration


@dataclass
class Configuration(BaseModel):
    camera: CameraConfiguration = field(default_factory=CameraConfiguration)
    stage: StageConfiguration = field(default_factory=StageConfiguration)
    scanner: ScannerConfiguration = field(default_factory=ScannerConfiguration)

    def update_z_correction_terms(self, index, blz=None):
        # TODO: this function should be moved somewhere else
        # supposes the scan surface is flat and non-vertical
        fl, br = self.scanner.zones[index].FL, self.scanner.zones[index].BR
        x, y, z = 0, 1, 2

        if br[x] == fl[x] or br[y] == fl[y]:
            print("brx == flx or bry == fly !!!")
            return

        if blz is None:
            blz = (fl[z] + br[z])//2
        
        dz_dx = (blz - fl[z]) / (br[x] - fl[x])
        dz_dy = (br[z] - blz) / (br[y] - fl[y])

        self.scanner.zones[index].BL_Z = blz
        self.scanner.zones[index].Z_corrections = [dz_dx, dz_dy]

    def save_default(self):
        self.save(os.path.join(os.path.expanduser("~"), ".sashimi", "config.json"), 4)

    @staticmethod
    def load_default():
        config_file = os.path.join(os.path.expanduser("~"), ".sashimi", "config.json")
        if Path(config_file).exists():
            return Configuration.open(config_file)
        else:
            return Configuration()
