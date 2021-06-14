using System;
using System.Drawing;

namespace ImageProcessing.Segmentation
{
    public class SquareWithinBounds
    {
        public Rectangle Rectangle { get; set; }
        public Point Centroid { get; set; }

        public SquareWithinBounds(Rectangle rectangle, Point centroid)
        {
            Rectangle = rectangle;
            Centroid = centroid;
        }

        public SquareWithinBounds(Point centroid, int halfWidth, Size bounds)
        {            
            int startX = Math.Max(centroid.X - halfWidth, 0);
            int startY = Math.Max(centroid.Y - halfWidth, 0);
            int endX = Math.Min(centroid.X + halfWidth, bounds.Width - 1);
            int endY = Math.Min(centroid.Y + halfWidth, bounds.Height - 1);
            Rectangle = new Rectangle(startX, startY, endX - startX + 1, endY - startY + 1);
            int newX = centroid.X < halfWidth ? centroid.X : halfWidth;
            int newY = centroid.Y < halfWidth ? centroid.Y : halfWidth;
            Centroid = new Point(newX, newY);
        }

        public SquareWithinBounds(PointF centroid, int halfWidth, Size bounds)
        {
            var X = (int)Math.Round(centroid.X);
            var Y = (int)Math.Round(centroid.Y);
            int startX = Math.Max(X - halfWidth, 0);
            int startY = Math.Max(Y - halfWidth, 0);
            int endX = Math.Min(X + halfWidth, bounds.Width - 1);
            int endY = Math.Min(Y + halfWidth, bounds.Height - 1);
            Rectangle = new Rectangle(startX, startY, endX - startX + 1, endY - startY + 1);
            int newX = X < halfWidth ? X : halfWidth;
            int newY = Y < halfWidth ? Y : halfWidth;
            Centroid = new Point(newX, newY);
        }
    }
}
