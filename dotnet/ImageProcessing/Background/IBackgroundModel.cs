using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using ImageProcessing.Segmentation;

namespace ImageProcessing.Background
{
    public interface IBackgroundModel
    {
        List<Mat> Models { get; }
        void Update(Mat mat, List<Contour> contours, int idx);
        void Initialise();
        Mat Model(int modelIdx);
        (Mat, PointF) Difference(Mat mat, int idx);
        event Action<IBackgroundModel, int> ModelUpdatedEvent;
    }
}