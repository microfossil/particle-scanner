import click
from os import path
from pathlib import Path
from sashimi import focus_stack
from sashimi import helicon_stack as helicon_stacker
from sashimi.controller import Controller
from sashimi.multi_exp import dialog_for_path_and_values

# TODO: refactor commands "scan" and "multi_exp" to a
#  single "scan" command with a "multi_exp" option


@click.group()
def cli():
    pass


@cli.command()
@click.option('--dir', '-d', 'dir_',
              type=str,
              prompt='Directory to save images',
              default=path.expanduser('~/images/sashimi/'),
              help='Directory to save images')
@click.option('--port', '-p',
              type=str,
              prompt='COM port of printer',
              default='COM5',
              help='COM port of the 3D printer')
@click.option('--lang', '-l',
              type=str,
              default="en",
              # prompt="Language",
              help='Language of the interface (en/fr)')
@click.option('--layout',
              type=str,
              default='QWERTY',
              help='Layout of the keyboard (QWERTY/AZERTY)')
@click.option("--mult-exp", "-e",
              is_flag=True,
              help="Allows to use multiple Exposures")
@click.option("--remove-pics", "-r",
              is_flag=True,
              help='Removes the non-stacked pictures after finishing stacking')
@click.option("--skip-fs", "-s",
              is_flag=True,
              help='disable the automatic focus-stacking of pictures after a scan')
@click.option('--auto-quit', '-q',
              is_flag=True,
              help='sashimi quits automatically after scanning')
@click.option('--offset', '-o',
              type=int,
              default=1000,
              help='z offset in top-down mode')
@click.option('--lowest', '-z',
              is_flag=True,
              help='simplifies z correction')
def scan(dir_, port, lang, layout, mult_exp, remove_pics, skip_fs, auto_quit, offset, lowest):
    if mult_exp:
        user_path, exp_values = dialog_for_path_and_values()
        print("Input collection finished, the scanning program will start.")
    else:
        user_path = dir_
        exp_values = None
        
    controller = Controller(user_path,
                            port,
                            lang=lang,
                            layout=layout,
                            multi_exp=exp_values,
                            remove_pics=remove_pics,
                            auto_f_stack=not skip_fs,
                            auto_quit=auto_quit,
                            reposition_offset=offset,
                            lowest_z=lowest)
    controller.start()


@cli.command()
@click.option('--dir', '-d', 'dir_',
              type=str,
              # prompt='Directory containing stacks',
              help='Directory containing subdirectories of image stacks')
def stack(dir_):
    focus_stack.stack(dir_)


@cli.command()
@click.option('--dir', '-d', 'dir_',
              type=str,
              # prompt='Directory containing stacks',
              help='Directory containing subdirectories of image stacks')
@click.option('--multi-exp', '--mx',
              type=int,
              nargs=-1,
              default=False,
              help="Allows to stack with folders containing exposure sub-folders.\n"
                   "The inputted values are the different exposures you wish to process")
def helicon_stack(dir_, mx):
    dir_ = Path(dir_).resolve()
    if mx:
        to_ = dir_.joinpath('f_stacks')
        helicon_stacker.stack_for_multiple_exp(dir_, to_, exp_values=mx)
    else:
        helicon_stacker.stack(dir_)


if __name__ == "__main__":
    cli()
