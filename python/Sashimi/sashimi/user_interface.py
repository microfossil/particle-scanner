import cv2
import numpy as np

class UserInterface:
    """
    User Interface class for displaying the camera image and user controls."""
    def __init__(self, controller):
        self.controller = controller
        self.keyboard = controller.keyboard
        self.stage = controller.stage
        self.config = controller.config
        self.scanner = controller.scanner

    def render(self, im: np.array):
        # Reduce size of image
        im = im[::4, ::4, :].astype(np.uint8)

        if self.controller.img_mode > 1:
            # Image mode 2, 3, 4 are red, green, and blue
            im = np.repeat(im[:, :, self.controller.img_mode - 2][..., np.newaxis], 3, axis=-1)

        # Add black space to the left of the image for UI text
        left_edge_size = 300
        bottom_edge_size = 30
        im = np.pad(im, [[0, bottom_edge_size], [left_edge_size, 0], [0, 0]])

        colors = [
            (100, 255, 255),  # Keyboard commands color
            (255, 255, 255),  # Text command color
            (50, 100, 255),  # Section color
        ]

        left_panel = self._get_text_left_panel()
        text_help = self._get_text_help()

        # Draw text on the image
        # self._draw_text(im, text_status, (50, 20), (230, 230, 230))
        # self._draw_text(im, text_button, (10, 20), (120, 255, 255))
        self._draw_text_left_panel(im, left_panel, (10, 20), colors)
        self._draw_text(im, text_help, (left_edge_size + 10, 20), (0, 255, 255))

        # Show the image
        cv2.imshow("im", im)

    def _get_text_left_panel(self):
        kb = self.keyboard
        position_text_1 = f"[X, Y, Z]: {[self.stage.x, self.stage.y, self.stage.z]}"
        position_text_2 = f"Home: {self.config.home_position}"
        exposure_text = f"Exposure: {self.config.exposure_time}us"
        height_text = f"Height: {self.config.stack_height}um"
        step_text = f"Step: {self.config.stack_step}um"
        zone_text = f"Zone: {self.controller.selected_scan_number}/{len(self.config.scans)}"
        front_left_text = f"FL: {self.controller.selected_scan()['FL']}"
        back_right_text = f"BR: {self.controller.selected_scan()['BR']}"
        back_left_text = f"BL: Z={self.controller.selected_scan()['BL_Z']}"
        scan_text = "Start scanning"
        add_zone_text = "Add new zone"
        del_zone_text = "Delete current zone"
        del_scans_text = "Delete all scans"
        scan_command = f"{chr(kb.SCAN)}"
        line_break = "- - - - - - - - - - - -"

        if self.scanner.is_multi_scanning:
            scan_text = "Stop scanning"
            left_panel_text = [
                ["POSITION"     , ""           ],
                [position_text_1, ""           ],
                [position_text_2, ""           ],
                [line_break     , ""           ],
                ["CAMERA"       , ""           ],
                [exposure_text  , ""           ],
                [line_break     , ""           ],
                ["STACK"        , ""           ],
                [height_text    , ""           ],
                [step_text      , ""           ],
                [line_break     , ""           ],
                ["SCAN"         , ""           ],
                [zone_text      , ""           ],
                [front_left_text, ""           ],
                [back_right_text, ""           ],
                [back_left_text , ""           ],
                ["add_zone_text", ""           ],
                ["del_zone_text", ""           ],
                [line_break     , ""           ],
                ["COMMANDS"     , ""           ],
                [scan_text      , scan_command ],
                ["del_scans_text", ""           ],
                [""             , ""           ],
                [""             , ""           ],
                ["quit"         , "esc"        ],
            ]
        else:
            position_command_1 = f"{chr(kb.FORWARD)} {chr(kb.BACK)} {chr(kb.LEFT)}"
            position_command_2 = f"{chr(kb.RIGHT)} {chr(kb.UP)} {chr(kb.DOWN)}"
            exposure_command = f"{chr(kb.EXPOSURE_UP)} {chr(kb.EXPOSURE_DOWN)}"
            set_home_command = f"{chr(kb.SET_HOME)}"
            zone_command = f"{chr(kb.PREV_SCAN)} {chr(kb.NEXT_SCAN)}"
            add_zone_command = f"{chr(kb.ADD_ZONE)}"
            del_zone_command = f"{chr(kb.DEL_ZONE)}"
            front_left_command = f"{chr(kb.SCAN_FL)}"
            back_right_command = f"{chr(kb.SCAN_BR)}"
            back_left_command = f"{chr(kb.SET_Z_COR)}"
            scan_command = f"{chr(kb.SCAN)}"
            del_scans_command = f"{chr(kb.DEL_ALL_ZONES)}"
            left_panel_text = [
                ["POSITION"     , position_command_1],
                [position_text_1, position_command_2],
                [position_text_2, set_home_command  ],
                [line_break     , ""                ],
                ["CAMERA"       , ""                ],
                [exposure_text  , exposure_command  ],
                [line_break     , ""                ],
                ["STACK"        , ""                ],
                [height_text    , "[ ]"             ],
                [step_text      , "{ }"             ],
                [line_break     , ""                ],
                ["SCAN"         , ""                ],
                [zone_text      , zone_command      ],
                [front_left_text, front_left_command],
                [back_right_text, back_right_command],
                [back_left_text , back_left_command ],
                [add_zone_text  , add_zone_command  ],
                [del_zone_text  , del_zone_command  ],
                [line_break     , ""                ],
                ["COMMANDS"     , ""                ],
                [scan_text      , scan_command      ],
                [del_scans_text , del_scans_command ],
                [""             , ""                ],
                [""             , ""                ],
                ["quit"         , "esc"             ],
            ]
        return left_panel_text

    def _get_text_help(self):
        if self.controller.show_help:
            return [
                "h/H: set/goto home position",
                "w,s,a,d,q,e: forward, back, left, right, up, down",
                "W,S,A,D,Q,E: 10 x forward, back, left, right, up, down",
                "[ ]: -/+ stack height (100um)",
                "{ }: -/+ stack step (20um)",
                "j/J: set/goto scan front left",
                "i/I: set/goto scan back right",
                "enter: take stack",
                "p: start/stop scan",
                "esc: quit",
                "?: close help",
            ]
        else:
            return ["?: show help"]

    def _draw_text_left_panel(self, im, my_list, start_pos, color):
        for i, lst in enumerate(my_list):
            text = lst[0]
            command = lst[1]
            if (text.isupper() and text.isalpha()) or text.startswith("-"):
                text_color = color[2]
            else:
                text_color = color[1]
            cv2.putText(
                im,
                command,
                (start_pos[0], start_pos[1] + i * 20),
                cv2.FONT_HERSHEY_SIMPLEX,
                0.4,
                color[0],
                1,
                cv2.LINE_AA,
            )
            cv2.putText(
                im,
                text,
                (start_pos[0] + 40, start_pos[1] + i * 20),
                cv2.FONT_HERSHEY_SIMPLEX,
                0.4,
                text_color,
                1,
                cv2.LINE_AA,
            )

    def _draw_text(self, im, text_list, start_pos, color):
        for i, text in enumerate(text_list):
            cv2.putText(
                im,
                text,
                (start_pos[0], start_pos[1] + i * 20),
                cv2.FONT_HERSHEY_SIMPLEX,
                0.4,
                color,
                1,
                cv2.LINE_AA,
            )
