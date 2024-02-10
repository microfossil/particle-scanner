import sys

from PySide6.QtCore import QThread, Qt
from PySide6.QtWidgets import QApplication, QMainWindow, QPushButton, QVBoxLayout, QLabel, QWidget, QHBoxLayout, \
    QDoubleSpinBox, QSpinBox, QGridLayout, QLayout, QTextEdit
from PySide6.QtGui import QImage, QPixmap
import numpy as np

from sashimi.configuration import Configuration
from sashimi.gui.controller import ControllerWorker


class MainWindow(QMainWindow):
    def __init__(self):
        super().__init__()

        # self.controller = controller
        self.setWindowTitle("Sashimi Controller Interface")

        # Controller
        self.worker = ControllerWorker()
        self.thread = QThread()
        self.worker.moveToThread(self.thread)
        self.worker.camera_image_changed.connect(self.update_image)

        # Connect key press signal to worker's slot
        # Assuming you have a mechanism in your GUI to capture key presses
        # self.keyPressedSignal.connect(self.worker.on_key_pressed)

        self.thread.start()

        self.image_label = QLabel("Camera Feed")
        self.init_ui()

    def init_ui(self):
        # Main
        layout = QHBoxLayout()

        controls_layout = QVBoxLayout()
        controls_widget = QWidget()
        controls_widget.setLayout(controls_layout)

        camera_layout = QVBoxLayout()
        image_widget = QWidget()
        image_widget.setLayout(camera_layout)

        layout.addWidget(controls_widget)
        layout.addWidget(image_widget)

        self.init_camera_ui(camera_layout)
        self.init_controls_ui(controls_layout)

        # Set the central widget
        central_widget = QWidget()
        central_widget.setLayout(layout)
        self.setCentralWidget(central_widget)

    def init_camera_ui(self, camera_layout):
        colour_mode_layout = QHBoxLayout()

        self.button_bgr = QPushButton("BGR")
        test = self.button_bgr.clicked.connect(self.worker.camera_bgr)
        print(test)
        button_blue = QPushButton("Blue")
        button_blue.clicked.connect(self.worker.camera_blue)
        button_green = QPushButton("Green")
        button_green.clicked.connect(self.worker.camera_green)
        button_red = QPushButton("Red")
        button_red.clicked.connect(self.worker.camera_red)

        colour_mode_layout.addWidget(self.button_bgr)
        colour_mode_layout.addWidget(button_blue)
        colour_mode_layout.addWidget(button_green)
        colour_mode_layout.addWidget(button_red)

        colour_mode_widget = QWidget()
        colour_mode_widget.setLayout(colour_mode_layout)
        camera_layout.addWidget(colour_mode_widget)

        exposure_and_gain_layout = QHBoxLayout()
        self.add_exposure_control(exposure_and_gain_layout)
        self.add_gain_control(exposure_and_gain_layout)
        self.add_layout_as_widget(exposure_and_gain_layout, camera_layout)

        camera_layout.addWidget(self.image_label)

    def add_layout_as_widget(self, layout, parent_layout):
        widget = QWidget()
        widget.setLayout(layout)
        parent_layout.addWidget(widget)

    def add_exposure_control(self, parent_layout):
        layout = QVBoxLayout()
        exposureLabel = QLabel("Exposure Time (ms):", self)
        layout.addWidget(exposureLabel)
        exposureSpinBox = QSpinBox(self)
        exposureSpinBox.setRange(100, 20000)
        exposureSpinBox.setSingleStep(500)
        exposureSpinBox.setValue(self.worker.config.exposure_time)
        layout.addWidget(exposureSpinBox)
        exposureSpinBox.valueChanged.connect(self.worker.camera_exposure)
        self.worker.camera_exposure_changed.connect(exposureSpinBox.setValue)
        widget = QWidget()
        widget.setLayout(layout)
        parent_layout.addWidget(widget)

    def add_gain_control(self, parent_layout):
        layout = QVBoxLayout()
        label = QLabel("Gain:", self)
        layout.addWidget(label)
        spinBox = QDoubleSpinBox(self)
        spinBox.setRange(0.0, 20.0)
        spinBox.setSingleStep(0.5)
        spinBox.setValue(self.worker.config.gain)
        layout.addWidget(spinBox)
        spinBox.valueChanged.connect(self.worker.camera_gain)
        self.worker.camera_gain_changed.connect(spinBox.setValue)
        widget = QWidget()
        widget.setLayout(layout)
        parent_layout.addWidget(widget)

    def init_controls_ui(self, controls_layout):


        self.add_movement_controls(controls_layout)
        self.add_scan_controls(controls_layout)
        self.add_config_information(controls_layout)

    def add_config_information(self, parent_layout):
        displayBox = QTextEdit()
        displayBox.setReadOnly(True)
        def update_config_information(config: Configuration):
            message = ""
            message += f"Home:\t{config.home_offset[0]}\t{config.home_offset[1]}\t{config.home_offset[2]}"
            message += "\n"
            for i, zone in enumerate(config.scanner.zones):
                if self.worker.selected_scan_zone == i:
                    message += f"\n\nScan {i}: [SELECTED]"
                else:
                    message += f"\n\nScan {i}:"
                message += (f"\nFL\t{zone.FL[0]}\t{zone.FL[1]}\t{zone.FL[2]}"
                            f"\nBR\t{zone.BR[0]}\t{zone.BR[1]}\t{zone.BR[2]}"
                            f"\nBL_Z\t{zone.BL_Z}"
                            f"\nZ_corrections\t{zone.Z_corrections[0]}\t{zone.Z_corrections[1]}")
            displayBox.setText(message)

        self.worker.config_changed.connect(update_config_information)
        update_config_information(self.worker.config)

        parent_layout.addWidget(displayBox)


    def add_scan_controls(self, parent_layout):
        layout = QVBoxLayout()

        label = QLabel()
        label.setText("Scanning")
        layout.addWidget(label)

        # Row 1: Select Previous, Select Next
        row1Layout = QHBoxLayout()
        btnPrevZone = QPushButton("Previous Zone")
        btnPrevZone.clicked.connect(self.worker.scan_select_previous_zone)
        row1Layout.addWidget(btnPrevZone)

        btnNextZone = QPushButton("Next Zone")
        btnNextZone.clicked.connect(self.worker.scan_select_next_zone)
        row1Layout.addWidget(btnNextZone)
        layout.addLayout(row1Layout)

        # Row 2: Add New, Set FL, Set BR, Set Z Correction
        row2Layout = QHBoxLayout()
        btnAddZone = QPushButton("New Zone")
        btnAddZone.clicked.connect(self.worker.scan_add_zone)
        row2Layout.addWidget(btnAddZone)

        btnSetFL = QPushButton("Set Front Left")
        btnSetFL.clicked.connect(self.worker.scan_set_FL)
        row2Layout.addWidget(btnSetFL)

        btnSetBR = QPushButton("Set Back Right")
        btnSetBR.clicked.connect(self.worker.scan_set_BR)
        row2Layout.addWidget(btnSetBR)

        btnSetZCor = QPushButton("Set Z Correction")
        btnSetZCor.clicked.connect(self.worker.scan_set_z_correction)
        row2Layout.addWidget(btnSetZCor)
        layout.addLayout(row2Layout)

        # Row 3: Delete Selected, Delete All
        row3Layout = QHBoxLayout()
        btnDeleteZone = QPushButton("Delete Zone")
        btnDeleteZone.clicked.connect(self.worker.scan_delete_zone)
        row3Layout.addWidget(btnDeleteZone)

        btnDeleteAllZones = QPushButton("Delete All Zones")
        btnDeleteAllZones.clicked.connect(self.worker.scan_delete_all_zones)
        row3Layout.addWidget(btnDeleteAllZones)
        layout.addLayout(row3Layout)

        start_scan_button = QPushButton("Start Scanning")
        start_scan_button.clicked.connect(self.worker.scanner.start)
        layout.addWidget(start_scan_button)

        self.add_layout_as_widget(layout, parent_layout)


    def add_movement_controls(self, parent_layout):
        grid = QGridLayout()
        grid.setSizeConstraint(QLayout.SetFixedSize)

        # Create and place buttons in a 5x6 grid
        for row in range(5):
            for col in range(6):
                # Place "Left" and "Right" in the middle axis (column 2 and 3)
                if row == 2 and col == 1:
                    button = QPushButton("Left")
                    button.clicked.connect(self.worker.stage_move_left)
                elif row == 2 and col == 3:
                    button = QPushButton("Right")
                    button.clicked.connect(self.worker.stage_move_right)
                # "X Left" and "X Right" next to "Left" and "Right" (columns 0 and 5)
                elif row == 2 and col == 0:
                    button = QPushButton("X Left")
                    button.clicked.connect(self.worker.stage_move_x_left)
                elif row == 2 and col == 4:
                    button = QPushButton("X Right")
                    button.clicked.connect(self.worker.stage_move_x_right)


                # Place "Forward" and "Back" in the middle axis (row 2 and 3)
                elif col == 2 and row == 1:
                    button = QPushButton("Forward")
                    button.clicked.connect(self.worker.stage_move_forward)
                elif col == 2 and row == 3:
                    button = QPushButton("Back")
                    button.clicked.connect(self.worker.stage_move_back)
                # "X Forward" and "X Back" next to "Forward" and "Back" (row 0 and 4, column 2)
                elif col == 2 and row == 0:
                    button = QPushButton("X Forward")
                    button.clicked.connect(self.worker.stage_move_x_forward)
                elif col == 2 and row == 4:
                    button = QPushButton("X Back")
                    button.clicked.connect(self.worker.stage_move_x_back)

                # Place "Up" and "Down" in the last column (row 2 and 3)
                elif col == 5 and row == 1:
                    button = QPushButton("Up")
                    button.clicked.connect(self.worker.stage_move_up)
                elif col == 5 and row == 3:
                    button = QPushButton("Down")
                    button.clicked.connect(self.worker.stage_move_down)
                # "X Up" and "X Down" below "Up" and "Down" (last column, row 2 and 4)
                elif col == 5 and row == 0:
                    button = QPushButton("X Up")
                    button.clicked.connect(self.worker.stage_move_x_up)
                elif col == 5 and row == 4:
                    button = QPushButton("X Down")
                    button.clicked.connect(self.worker.stage_move_x_down)

                elif col == 0 and row == 0:
                    button = QPushButton("Zero")
                    button.clicked.connect(self.worker.stage_home)

                else:
                    continue
                grid.addWidget(button, row, col)

        self.add_layout_as_widget(grid, parent_layout)



    def update_image(self, image: np.ndarray):
        # Convert image to QImage and display
        q_image = QImage(image.data, image.shape[1], image.shape[0], QImage.Format_RGB888)
        self.image_label.setPixmap(QPixmap.fromImage(q_image))

    def closeEvent(self, event):
        # Signal the worker to stop, if you have a mechanism in the worker to do so
        self.worker.stop()

        # Wait for the thread to finish
        self.thread.quit()
        self.thread.wait()

        # Proceed with the rest of the shutdown process
        super().closeEvent(event)

if __name__ == "__main__":
    # Assuming 'controller' is an instance of your Controller class
    app = QApplication(sys.argv)

    window = MainWindow()  # You need to create and pass the controller instance here
    window.show()

    sys.exit(app.exec())
