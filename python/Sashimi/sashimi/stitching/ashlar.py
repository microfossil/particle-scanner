import ashlar

# Assuming you have a directory of tiled images to stitch...
input_directory = 'path/to/your/images'
output_file = 'stitched_image.ome.tif'

ashlar.aln

# Create an aligner object
aln = aligner.Aligner(input_directory)

# Perform alignment and stitching
aln.align()

# Save the stitched image
aln.save(output_file)