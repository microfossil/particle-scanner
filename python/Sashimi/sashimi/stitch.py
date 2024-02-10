from stitch2d import create_mosaic

from stitch2d import StructuredMosaic

mosaic = StructuredMosaic(
    r"F:\Sashimi\test\zone_00000",
    dim=3,                  # number of tiles in primary axis
    origin="lower left",     # position of first tile
    direction="vertical",  # primary axis (i.e., the direction to traverse first)
    pattern="raster"          # snake or raster
  )


# mosaic = create_mosaic(r"F:\Sashimi\test\zone_00000")

mosaic.align()
mosaic.reset_tiles()
mosaic.save_params()

mosaic.smooth_seams()
mosaic.save(r"F:\Sashimi\test\zone_00000\mosaic\mosaic.jpg")