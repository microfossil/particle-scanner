import click
from sashimi import focus_stack
from sashimi import helicon_stack as helicon_stacker
from sashimi.controller import Controller


@click.group()
def cli():
    pass


@cli.command()
@click.argument(dcls=[None, '--dir', '-d', 'dir_'],
                type=str,
                required=True,
                prompt='Directory to save images',
                help='Directory to save images')
@click.argument(dcls=[None, '--port', '-p'],
                type=str,
                required=True,
                prompt='COM port of printer',
                help='COM port of the 3D printer')
@click.option(dcls=['--lang', '-l'],
              type=str,
              default="en",
              help='Language of the interface (en/fr)')
@click.option(dcls=['--offset', '-o'],
              type=int,
              default=1000,
              help='z offset in top-down mode')
@click.option(dcls=['--lowest', '-z'],
              is_flag=True,
              flag_value=True,
              default=False,
              help='simplifies z correction')
def scan(dir_, port, lang, offset, lowest):
    controller = Controller(dir_,
                            port,
                            lang=lang,
                            reposition_offset=offset,
                            lowest_z=lowest)
    controller.start()


@cli.command()
@click.option('--dir',
              type=str,
              prompt='Directory containing stacks',
              help='Directory containing subdirectories of image stacks')
def stack(dir):
    focus_stack.stack(dir)


@cli.command()
@click.option('--dir',
              type=str,
              prompt='Directory containing stacks',
              help='Directory containing subdirectories of image stacks')
def helicon_stack(dir):
    helicon_stacker.stack(dir)


if __name__ == "__main__":
    cli()
