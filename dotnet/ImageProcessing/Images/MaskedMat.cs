using System.Drawing;
using Emgu.CV;
using Emgu.CV.Util;

namespace ImageProcessing.Images
{
    public class MaskedMat
    {
        public Mat Mat;
        public Mat Mask;
        public VectorOfPoint MaskContour;
        public Point Centroid;
        public double Area = 0;

        public MaskedMat(Mat mat, Mat mask, VectorOfPoint maskContour, Point centroid)
        {
            Mat = mat;
            Mask = mask;
            MaskContour = maskContour;
            Centroid = centroid;
        }

        public void Release()
        {
            Mat.Dispose();
            Mask.Dispose();
            MaskContour.Dispose();
        }
    }
}
