import cv2
import numpy as np
from sashimi.utils import get_project_version
from sashimi.controller_states import State

class UserInterface:
    """
    User Interface class for displaying the camera image and user controls."""
    def __init__(self, controller):
        self.controller = controller
        self.keyboard = controller.keyboard
        self.stage = controller.stage
        self.config = controller.config
        self.scanner = controller.scanner
        self.orange = (50, 150, 255)
        self.yellow = (100, 255, 255)
        self.white = (255, 255, 255)
        self.red = (75, 75, 255)
        self.green = (100, 255, 100)
        self.cyan = (255, 255, 0)
        self.left_panel_width = 300
        self.bottom_edge_size = 30

        self.nb_dots = 0
        self.loading_dots = ""
        self.update_nb_dots = 0  # To control update frequenc

    def render(self, im: np.array):
        # Reduce size of image
        im = im[::4, ::4, :].astype(np.uint8)

        if self.controller.img_mode > 1:
            # Image mode 2, 3, 4 are red, green, and blue
            im = np.repeat(im[:, :, self.controller.img_mode - 2][..., np.newaxis], 3, axis=-1)

        # Add black space to the left of the image for UI text

        im = np.pad(im, [[0, self.bottom_edge_size], [self.left_panel_width, 0], [0, 0]])

        # Loading dot cycling computation
        # -------------------------------------
        if self.controller.state != State.IDLE:
            if self.update_nb_dots % 10 == 0:  # Update every 10 frames
                self.nb_dots = ((self.nb_dots + 1) % 4)  # Cycle through 0, 1, 2, 3
                self.loading_dots = "." * self.nb_dots
            self.update_nb_dots += 1
        else:
            self.loading_dots = ""
        # -------------------------------------

        # Get text to display in the UI
        # -------------------------------------
        left_panel_txt, left_panel_exit = self._get_left_panel_txt()
        text_help = self._get_text_help()
        # -------------------------------------

        # Define colors for the UI
        # -------------------------------------
        left_panel_colors = [
            self.yellow, # Keyboard commands color
            self.white,  # Text command color
            self.orange, # Section color
        ]
        if self.controller.state == State.IDLE:
            state_color = [self.orange, self.green]
        else:
            state_color = [self.orange, self.red]

        version_color = [self.orange, self.white]
        # -------------------------------------

        # Display the UI
        # -------------------------------------
        self._draw_txt_left_panel(im, left_panel_txt, (10, 20), 50, left_panel_colors)
        self._draw_txt_left_panel(im, left_panel_exit, (10, im.shape[0]-10), 50, left_panel_colors)
        self._draw_text(im, text_help, (self.left_panel_width + 10, 20), self.cyan)
        self._draw_txt_key_value(
            im,
            [["Printer state: ", self.controller.state.value + self.loading_dots]],
            (self.left_panel_width + 10, im.shape[0]-10),
            90,
            state_color
        )
        self._draw_txt_key_value(
            im,
            [['Version:', get_project_version()]],
            (im.shape[1]-110, im.shape[0]-10),
            55,
            version_color
        )
        # -------------------------------------

        # Show the image
        cv2.imshow("im", im)

    def _get_left_panel_txt(self):
        """Get the text to display in the left panel of the UI"""
        kb = self.keyboard

        position_command_1 = f"{chr(kb.FORWARD)} {chr(kb.BACK)} {chr(kb.LEFT)}"
        position_command_2 = f"{chr(kb.RIGHT)} {chr(kb.UP)} {chr(kb.DOWN)}"
        exposure_command = f"{chr(kb.EXPOSURE_DOWN)} {chr(kb.EXPOSURE_UP)}"
        set_home_command = f"{chr(kb.SET_HOME)}"
        zone_command = f"{chr(kb.PREV_SCAN)} {chr(kb.NEXT_SCAN)}"
        add_zone_command = f"{chr(kb.ADD_ZONE)}"
        del_zone_command = f"{chr(kb.DEL_ZONE)}"
        front_left_command = f"{chr(kb.SCAN_FL)}"
        back_right_command = f"{chr(kb.SCAN_BR)}"
        back_left_command = f"{chr(kb.SET_Z_COR)}"
        del_scans_command = f"{chr(kb.DEL_ALL_ZONES)}"
        scan_command = f"{chr(kb.SCAN)}"
        home_command = f"{chr(kb.HOME)}"
        auto_level_command = f"{chr(kb.AUTO_LEVEL)}"

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
        del_scans_text = "Restore scans settings"
        home_text = "Move to home position"
        auto_level_text = "Auto level printer"
        line_break = "- - - - - - - - - - - -"

        # Left panel displayed when printer is initializing
        if self.controller.state == State.INIT:
            left_panel_txt = [
                [ ""          , "POSITION"     ],
                [ ""          , position_text_1],
                [ ""          , position_text_2],
                [ ""          , line_break     ],
                [ ""          , "CAMERA"       ],
                [ ""          , exposure_text  ],
                [ ""          , line_break     ],
                [ ""          , "STACK"        ],
                [ ""          , height_text    ],
                [ ""          , step_text      ],
                [ ""          , line_break     ],
                [ ""          , "SCAN"         ],
                [ ""          , zone_text      ],
                [ ""          , front_left_text],
                [ ""          , back_right_text],
                [ ""          , back_left_text ],
                [ ""          , ""             ],
                [ ""          , ""             ],
                [ ""          , ""             ],
            ]
        # Left panel displayed when printer is scanning
        elif self.controller.state == State.SCAN:
            scan_text = "Stop scanning"
            left_panel_txt = [
                [ ""          , "POSITION"     ],
                [ ""          , position_text_1],
                [ ""          , position_text_2],
                [ ""          , line_break     ],
                [ ""          , "CAMERA"       ],
                [ ""          , exposure_text  ],
                [ ""          , line_break     ],
                [ ""          , "STACK"        ],
                [ ""          , height_text    ],
                [ ""          , step_text      ],
                [ ""          , line_break     ],
                [ ""          , "SCAN"         ],
                [ ""          , zone_text      ],
                [ ""          , front_left_text],
                [ ""          , back_right_text],
                [ ""          , back_left_text ],
                [ ""          , ""             ],
                [ ""          , ""             ],
                [ ""          , ""             ],
                [ ""          , line_break     ],
                [ ""          , "COMMANDS"     ],
                [ scan_command, scan_text      ],
            ]
        else:
            left_panel_txt = [
                [position_command_1, "POSITION"     ],
                [position_command_2, position_text_1],
                [set_home_command  , position_text_2],
                [""                , line_break     ],
                [""                , "CAMERA"       ],
                [exposure_command  , exposure_text  ],
                [""                , line_break     ],
                [""                , "STACK"        ],
                ["[ ]"             , height_text    ],
                ["{ }"             , step_text      ],
                [""                , line_break     ],
                [""                , "SCAN"         ],
                [zone_command      , zone_text      ],
                [front_left_command, front_left_text],
                [back_right_command, back_right_text],
                [back_left_command , back_left_text ],
                [add_zone_command  , add_zone_text  ],
                [del_zone_command  , del_zone_text  ],
                [del_scans_command , del_scans_text ],
                [""                , line_break     ],
                [""                , "COMMANDS"     ],
                [scan_command      , scan_text      ],
                [home_command      , home_text      ],
                [auto_level_command, auto_level_text],
            ]
        left_panel_exit = [
                ["quit"         , "esc"        ]
                ]
        return left_panel_txt, left_panel_exit

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

    def _draw_txt_left_panel(self, im, my_list, start_pos, space_between, color):
        for i, lst in enumerate(my_list):
            command = lst[0]
            text = lst[1]
            if (text.isupper() and text.isalpha()) or text.startswith("-"):
                text_color = color[2]
            else:
                text_color = color[1]
            self.put_text(
                im, 
                command, 
                (start_pos[0], start_pos[1] + i * 20), 
                color[0]
            )
            self.put_text(
                im,
                text,
                (start_pos[0] + space_between, start_pos[1] + i * 20),
                text_color
            )

    def _draw_txt_key_value(self, im, my_list, start_pos, space_between, color):
        for i, lst in enumerate(my_list):
            key = lst[0]
            value = lst[1]
            self.put_text(
                im, 
                key, 
                (start_pos[0], start_pos[1] + i * 20), 
                color[0]
            )
            self.put_text(
                im, 
                value, 
                (start_pos[0] + space_between, start_pos[1] + i * 20), 
                color[1]
            )

    def _draw_text(self, im, text_list, start_pos, color):
        for i, text in enumerate(text_list):
            self.put_text(
                im, 
                text, 
                (start_pos[0], start_pos[1] + i * 20), 
                color
            )

    def put_text(self, im, text, position, color):
        cv2.putText(
            im,
            text,
            position,
            cv2.FONT_HERSHEY_SIMPLEX,
            0.4,
            color,
            1,
            cv2.LINE_AA,
        )
