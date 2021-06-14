using System;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.Util;

namespace ImageProcessing.Extensions
{
    public static class MorphologyExtensions
    {
        public static (double min, double max) MinMax(this Mat mat)
        {
            double min = 0, max = 0;
            Point minLoc = new Point(), maxLoc = new Point();
            CvInvoke.MinMaxLoc(mat, ref min, ref max, ref minLoc, ref maxLoc);
            return (min, max);
        }

        public static (double min, double max) MinMax(this Mat mat, Mat mask)
        {
            double min = 0, max = 0;
            Point minLoc = new Point(), maxLoc = new Point();
            CvInvoke.MinMaxLoc(mat, ref min, ref max, ref minLoc, ref maxLoc, mask);
            return (min, max);
        }

        public static double EllipseOrientation(this Moments moments)
        {
            var uxx = moments.Mu20;
            var uyy = moments.Mu02;
            var uxy = moments.Mu11;
            var common = Math.Sqrt(Math.Pow((uxx - uyy), 2) + 4 * Math.Pow(uxy, 2));
            double orientation = 0;
            double num;
            double den;
            if (uyy > uxx)
            {
                num = uyy - uxx + common;
                den = 2 * uxy;
            }
            else
            {
                num = 2 * uxy;
                den = uxx - uyy + common;
            }
            if (num == 0 && den == 0)
            {
                orientation = 0;
            }
            else
            {
                orientation = Math.Atan(num / den);
            }
            return orientation;
        }

        public static PointF CentroidF(this Moments moments)
        {
            var centreX = moments.M10 / moments.M00;
            var centreY = moments.M01 / moments.M00;
            return new PointF((float)centreX, (float)centreY);
        }

        public static Point Centroid(this Moments moments)
        {
            var centreX = (int)Math.Round(moments.M10 / moments.M00);
            var centreY = (int)Math.Round(moments.M01 / moments.M00);
            return new Point(centreX, centreY);
        }

        public static (double, double) MaxRadius(this VectorOfPoint contour, PointF centroid)
        {
            var points = contour.ToArray();
            double maxRadius = 0;
            double maxAngle = 0;
            foreach (PointF point in points)
            {
                var (radius, angle) = RadialVector(point, centroid);
                if (radius > maxRadius)
                {
                    maxRadius = radius;
                    maxAngle = angle;
                }
            }
            return (maxRadius, maxAngle);
        }

        public static (double, double) RadialVector(this PointF p1, PointF p2)
        {
            var dx = (double)p1.X - p2.X;
            var dy = (double)p1.Y - p2.Y;
            var radius = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            var angle = Math.Atan2(dy, dx);
            return (radius, angle);
        }
    }
}
