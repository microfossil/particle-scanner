import click
from sashimi import focus_stack
from sashimi import helicon_stack as helicon_stacker
from sashimi.controller import Controller

@click.group()
def cli():
    pass


@cli.command()
@click.option('--dir',
              type=str,
              prompt='Directory to save images',
              help='Directory to save images')
@click.option('--port',
              type=str,
              prompt='COM port of printer',
              help='COM port of the 3D printer')
@click.option('--lang',
              type=str,
              default="en",
              help='Language of the interface (en/fr)')
def scan(dir, port, lang):
    controller = Controller(dir, port, lang)
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