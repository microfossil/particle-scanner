using System.Collections.Generic;
using System.Drawing;
using Emgu.CV.Util;

namespace ImageProcessing.Extensions
{
    public static class ContourExtensions
    {
        public static VectorOfPoint Translate(this VectorOfPoint contour, Size shift)
        {
            List<Point> translated = new List<Point>(contour.Size);
            for (int i = 0; i < contour.Size; i++)
            {
                Point p = Point.Add(contour[i], shift);
                translated.Add(p);
            }
            return new VectorOfPoint(translated.ToArray());
        }
        
        public static VectorOfPoint Clone(this VectorOfPoint contour)
        {
            List<Point> cloned = new List<Point>(contour.Size);
            for (int i = 0; i < contour.Size; i++)
            {
                Point p = new Point(contour[i].X, contour[i].Y);
                cloned.Add(p);
            }
            return new VectorOfPoint(cloned.ToArray());
        }
    }
}
