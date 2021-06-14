using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using ImageProcessing.Extensions;

namespace ImageProcessing.Images
{
    public class StackedMat
    {
        public Mat Greyscale { get; set; }
        public Mat Colour { get; set; }
        public Mat Depth { get; set; }
        public Mat Background { get; set; }
        public Mat Mask { get; set; }
        public List<Mat> Stack { get; set; }

        public StackedMat()
        {

        }

        public StackedMat(Mat greyscale, Mat colour, Mat depth, List<Mat> stack)
        {
            Greyscale = greyscale;
            Colour = colour;
            Depth = depth;
            Stack = stack;
        }

        public StackedMat CreateCrop(Rectangle rect)
        {
            StackedMat crop = new StackedMat()
            {
                Greyscale = this.Greyscale?.Crop(rect),
                Colour = this.Colour?.Crop(rect),
                Depth = this.Depth?.Crop(rect),
                Background = this.Background?.Crop(rect),
                Mask = this.Mask?.Crop(rect),
                Stack = new List<Mat>(this.Stack.Select(x => x.Crop(rect)))
            };
            return crop;
        }

        public void Release()
        {
            if (Greyscale != null) Greyscale.Dispose();
            if (Colour != null) Colour.Dispose();
            if (Depth != null) Depth.Dispose();
            if (Background != null) Background.Dispose();
            if (Mask != null) Mask.Dispose();
            if (Stack != null)
            {
                foreach (Mat mat in Stack)
                {
                    mat.Dispose();
                }
            }
        }
    }
}
