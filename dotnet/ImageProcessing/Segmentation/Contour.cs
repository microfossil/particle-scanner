using System.Drawing;
using Emgu.CV;
using Emgu.CV.Util;
using ImageProcessing.Extensions;

namespace ImageProcessing.Segmentation
{
    public class Contour
    {
        public VectorOfPoint Points;
        public double Area;
        public double Perimeter;
        public double ConvexPerimeter;
        public double ConvexityRatio;
        public Point Centroid;
        public PointF CentroidF;

        public Contour(VectorOfPoint contour)
        {
            Points = contour.Clone();
            Calculate();
        }

        private void Calculate()
        {
            //Area
            Area = CvInvoke.ContourArea(Points);

            //Perimeter
            Perimeter = CvInvoke.ArcLength(Points, true);

            //Centroid
            Moments moments = CvInvoke.Moments(Points);
            CentroidF = moments.CentroidF();
            Centroid = moments.Centroid();

            //Convexity
            VectorOfPoint hull = new VectorOfPoint();
            CvInvoke.ConvexHull(Points, hull);
            ConvexPerimeter = CvInvoke.ArcLength(hull, true);
            ConvexityRatio = ConvexPerimeter / Perimeter;
            hull.Dispose();
        }
    }
}
