import click
from sashimi import focus_stack
from sashimi import helicon_stack
from sashimi.controller import Controller

@click.group()
def cli():
    pass


@cli.command()
@click.option('--output',
              type=str,
              prompt='Directory to save images',
              help='Directory to save images')
@click.option('--port',
              type=str,
              prompt='COM port of printer',
              help='COM port of the 3D printer')
def scan(output, port):
    controller = Controller(output, port)
    controller.start()


@cli.command()
@click.option('--input',
              type=str,
              prompt='Directory containing stacks',
              help='Directory containing subdirectories of image stacks')
def stack(input):
    focus_stack.stack(input)


@cli.command()
@click.option('--input',
              type=str,
              prompt='Directory containing stacks',
              help='Directory containing subdirectories of image stacks')
def helicon_stack(output):
    helicon_stack.stack(output)


if __name__ == "__main__":
    cli()