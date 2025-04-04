import cv2
import numpy as np

class UserInterface:
    def __init__(self, controller):
        self.controller = controller
        self.keyboard = controller.keyboard
        self.lang = controller.lang
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
        LEFT_EDGE_SIZE = 300
        im = np.pad(im, [[0, 0], [LEFT_EDGE_SIZE, 0], [0, 0]])

        sel_scan_num = self.controller.selected_scan_number
        sel_scan = self.controller.selected_scan()
        blz = sel_scan['BL_Z']

        # Define text for status and help
        text_status = self._get_text_status(sel_scan_num, sel_scan, blz)
        text_button = self._get_text_button()
        text_help = self._get_text_help()

        # Draw text on the image
        self._draw_text(im, text_status, (50, 20), (230, 230, 230))
        self._draw_text(im, text_button, (10, 20), (120, 255, 255))
        self._draw_text(im, text_help, (LEFT_EDGE_SIZE + 10, 20), (0, 255, 255))

        # Show the image
        cv2.imshow("im", im)

    def _get_text_status(self, sel_scan_num, sel_scan, blz):
        scan_command = "Stop scanning" if self.scanner.is_multi_scanning else "Start scanning"
        if self.lang == "en":
            return [
                "POSITION",
                f"[X, Y, Z]: {[self.stage.x, self.stage.y, self.stage.z]}",
                f"Home: {self.config.home_position}",
                "- - - - - - - - - - - -",
                "CAMERA",
                f"Exposure: {self.config.exposure_time}us",
                "- - - - - - - - - - - -",
                "STACK",
                f"Height: {self.config.stack_height}um",
                f"Step: {self.config.stack_step}um",
                "- - - - - - - - - - - -",
                "SCAN",
                f"Zone: {sel_scan_num}/{len(self.config.scans)}",
                f"FL: {sel_scan['FL']}",
                f"BR: {sel_scan['BR']}",
                f"BL: Z={blz}",
                "- - - - - - - - - - - -",
                "COMMANDS",
                scan_command,
                "Add new zone",
                "Delete current zone",
                "Delete all scans",
                "",
                "",
                "quit",
            ]
        elif self.lang == "fr":
            return [
                "POSITION",
                f"[X, Y, Z]: {[self.stage.x, self.stage.y, self.stage.z]}",
                f"Origine: {self.config.home_position}",
                "- - - - - - - - - - - -",
                "CAMERA",
                f"Exposition: {self.config.exposure_time}us",
                "- - - - - - - - - - - -",
                "PILE",
                f"Hauteur: {self.config.stack_height}um",
                f"Etape: {self.config.stack_step}um",
                "- - - - - - - - - - - -",
                "SCAN",
                f"Zone: {sel_scan_num}/{len(self.config.scans)}",
                f"AvGch: {sel_scan['FL']}",
                f"ArDt: {sel_scan['BR']}",
                f"ArGch: Z={blz}",
                "- - - - - - - - - - - -",
                "DEMANDES",
                scan_command,
                "Ajouter une zone",
                "Suppr. la zone",
                "Suppr. toutes les scans",
                "",
                "",
                "quitter",
            ]

    def _get_text_button(self):
        if self.scanner.is_multi_scanning:
            return [""] * 17 + ["p"] + [""] * 6 + ["esc"]
        else:
            kb = self.keyboard
            return [
                f"{chr(kb.FORWARD)} {chr(kb.BACK)} {chr(kb.LEFT)}",
                f"{chr(kb.RIGHT)} {chr(kb.UP)} {chr(kb.DOWN)}",
                "h",
                "",
                "",
                "g t",
                "",
                "",
                "[ ]",
                "{ }",
                "",
                "",
                f"{chr(kb.PREV_SCAN)} {chr(kb.NEXT_SCAN)}",
                "j",
                "i",
                "u",
                "",
                "",
                "p",
                "v",
                "B",
                "N",
                "",
                "",
                "esc",
            ]

    def _get_text_help(self):
        if self.controller.show_help:
            if self.lang == "en":
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
            elif self.lang == "fr":
                return [
                    "h/H: fixer/aller a la position d'origine",
                    "z,s,q,d,a,e: avant, arriere, gauche, droit, haut, bas",
                    "Z,S,Q,D,A,E: 10 x avant, arriere, gauche, droit, haut, bas",
                    "[ ]: -/+ hauteur de pile (100um)",
                    "{ }: -/+ etape de pile (20um)",
                    "j/J: fixer/aller a l'avant-gauche de la zone de scan",
                    "i/I: fixer/aller a l'arriere droit de la zone de scan",
                    "entree: faire une pile",
                    "p:  demarrer/arreter un scan",
                    "esc: quitter",
                    "?: se fermer",
                ]
        else:
            return ["?: show help"] if self.lang == "en" else ["?: afficher l'aide"]

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