import click
from os import path
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
@click.argument(dcls=[None, '--dir', '-d', 'dir_'],
                type=str,
                default=path.expanduser('~/images/sashimi/'),
                prompt='Directory to save images',
                help='Directory to save images')
@click.argument(dcls=[None, '--port', '-p'],
                type=str,
                default='COM5',
                prompt='COM port of printer',
                help='COM port of the 3D printer')
@click.option(dcls=['--lang', '-l'],
              type=str,
              default="en",
              prompt="Language",
              help='Language of the interface (en/fr)')
@click.option('--layout',
              type=str,
              default='QWERTY',
              help='Layout of the keyboard (QWERTY/AZERTY)')
@click.option(dcls=["--remove-pics", "-r"],
              is_flag=True,
              help='Removes the non-stacked pictures after finishing stacking')
@click.option(dcls=["--skip-fs", "-s"],
              is_flag=True,
              help='disable the automatic focus-stacking of pictures after a scan')
@click.option(dcls=['--auto-quit', '-q'],
              is_flag=True,
              help='sashimi quits automatically after scanning')
@click.option(dcls=['--offset', '-o'],
              type=int,
              default=1000,
              help='z offset in top-down mode')
@click.option(dcls=['--lowest', '-z'],
              is_flag=True,
              help='simplifies z correction')
def scan(dir_, port, lang, layout, remove_pics, skip_fs, auto_quit, offset, lowest):
    controller = Controller(dir_,
                            port,
                            lang=lang,
                            layout=layout,
                            remove_pics=remove_pics,
                            auto_f_stack=not skip_fs,
                            auto_quit=auto_quit,
                            reposition_offset=offset,
                            lowest_z=lowest)
    controller.start()


@cli.command()
@click.argument(dcls=[None, '--port', '-p'],
                type=str,
                default='COM5',
                prompt='COM port of printer',
                help='COM port of the 3D printer')
@click.option(dcls=['--lang', '-l'],
              type=str,
              default="en",
              prompt="Language",
              help='Language of the interface (en/fr)')
@click.option('--layout',
              type=str,
              default='QWERTY',
              prompt=True,
              help='Layout of the keyboard (QWERTY/AZERTY)')
@click.option(dcls=["--remove-pics", "-r"],
              is_flag=True,
              help='Removes the non-stacked pictures after finishing stacking')
@click.option(dcls=["--skip-fs", "-s"],
              is_flag=True,
              help='disable the automatic focus-stacking of pictures after a scan')
@click.option(dcls=['--auto-quit', '-q'],
              is_flag=True,
              help='sashimi quits automatically after scanning')
@click.option(dcls=['--offset', '-o'],
              type=int,
              default=1000,
              help='z offset in top-down mode')
@click.option(dcls=['--lowest', '-z'],
              is_flag=True,
              help='simplifies z correction')
def multiple_exp(port, lang, layout, remove_pics, skip_fs, auto_quit, offset, lowest):
    user_path, exp_values = dialog_for_path_and_values()
    print("Input collection finished, the scanning program will start.")
    controller = Controller(user_path,
                            port,
                            lang=lang,
                            layout=layout,
                            remove_pics=remove_pics,
                            multi_exp=exp_values,
                            auto_f_stack=not skip_fs,
                            auto_quit=auto_quit,
                            reposition_offset=offset,
                            lowest_z=lowest)
    controller.start()


@cli.command()
@click.option(dcls=[None, '--dir', '-d', 'dir_'],
              type=str,
              prompt='Directory containing stacks',
              help='Directory containing subdirectories of image stacks')
def stack(dir_):
    focus_stack.stack(dir_)


@cli.command()
@click.option(dcls=[None, '--dir', '-d', 'dir_'],
              type=str,
              prompt='Directory containing stacks',
              help='Directory containing subdirectories of image stacks')
def helicon_stack(dir_):
    helicon_stacker.stack(dir_)


if __name__ == "__main__":
    cli()
