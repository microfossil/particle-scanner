# 3D printer particle scanner

## Parts List

### 3D Printer
We use the cheap open-source Creality Ender 3 3D printer as the stage

Either the [Creality Ender 3 Pro](https://www.creality3dofficial.com/collections/ender-series/products/creality-ender-3-pro-3d-printer) or Creality Ender 3 V2 (https://www.creality3dofficial.com/collections/ender-series/products/ender-3-v2-3d-printer)

You will also need some **1.75mm PLA** filament for printing if you do not have any already.

| Part | Supplier | Part Number | Description | Link |
| --- | --- | --- | --- | --- |
| 3D Printer | Creality | Ender 3 Pro | Cheap 3D printer | https://www.creality3dofficial.com/collections/ender-series/products/creality-ender-3-pro-3d-printer |
| Filament | Creality | 1kg Ender PLA Filament | Black PLA filament used for 3D printing | https://www.creality3dofficial.com/products/1kg-ender-pla-filament |
| USB Cable | Any | Any | USB 2.0  cable (A Male to Mini B) for connecting computer to printer | e.g. https://www.amazon.com/AmazonBasics-USB-2-0-Cable-Male/dp/B00NH11N5A |

### Imaging system

| Part | Supplier | Part Number | Description | Link |
| --- | --- | --- | --- | --- |
| Camera | Basler | acA2440-35uc | 5MP USB3 colour machine vision camera | https://www.baslerweb.com/en/products/cameras/area-scan-cameras/ace/aca2440-35uc/ |
| USB Cable | Any | Any | USB 3.0 cable (A Male to Micro B) for communication with camera | e.g. https://www.amazon.com/AmazonBasics-USB-3-0-Cable-Male/dp/B00NH12R1O |
| Objective | VS Technology | VS-TCH4-65 | 4x magnification telecentric lens | https://vst.co.jp/en/machine-vision-lenses-en/vs-tch-series/ |
| Lighting | VS Technology | VL-LR2550W | White ring lighting | https://vst.co.jp/en/lighting-en/vl-lr-series/ |
| Lighting Power Supply | Any | Any | 24V 1A power supply with 2.1mm DC plug | N/A |
| Adapter | Any | Any | 2.1mm DC socket to screw terminal | e.g. https://www.amazon.com/Connector-Adapter-JEEUE-Female-Security/dp/B07SVD4PC3 |

### Misc.

* 2 x 10mm M3 bolts
* 3 x 20mm M3 bolts
* 5 x M3 washer
* 1 x 15mm M4 bolt
* 1 x M4 nut
* Wire capable of 1A
* Soldering equipment
* Power board with individual switches

## Build Instructions

### 1. Build 3D Printer

Follow the Creality manufacturers instructions to build the 3D printer

This youtube video shows the overall process. Pay attention from 10:00 onwards to how to adjust the nuts to tighten up the bed wobble.

https://www.youtube.com/watch?v=gokN9xNG94U

Note the following tricks:

* When levelling the bed, make sure the knobs under the bed have about 7mm of thread out the bottom. This is roughly the middle and will give enough room for later adjustments.
* Check if the bed has some wobble. If it does, tighten the hex nuts that attach it to the rail (see video above)

### 2. Build the camera and lighting holders

Print these two parts (located in the components directory)

* LINK TO PART
* LINK TO PART

### 3. Assemble camera, objective and lighting

VIDEO TO COME

### 4. Attach to 3D Printer

VIDEO TO COME

**DONE**

## Software Installation

Currently the software is written for the **WINDOWS 10** operating system

### 1. Install Basler Pylon

Install the [Basler Pylon Camera Software Suite](https://www.baslerweb.com/en/sales-support/downloads/software-downloads/#type=pylonsoftware;language=all;version=all). If prompted for which driver to install, select USB3.

Confirm the installation has worked by connecting the camera to your computer using the USB3 cable and running **pylon Viewer**

### 2. Install SASHIMI2 software

Install the XXX software from the XXX directory in this repository

## Usage

TO COME



