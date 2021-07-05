using System;
using System.Windows.Media.Media3D;
using Emgu.CV;

namespace MicroscopeImaging.Models
{
    public class ScannerImage : IDisposable
    {
        public Mat Mat { get; set; }
        public Point3D Coords { get; set; }
        public Point3D Index { get; set; }
        public double Scale { get; set; }
        public Mat Mask { get; set; }

        public ScannerImage(Mat mat)
        {
            this.Mat = mat;
        }

        public void Dispose()
        {
            Mat?.Dispose();
            Mask?.Dispose();
        }
    }

    public class ScannerSegment : IDisposable
    {
        public Mat Mat { get; set; }
        
        public void Dispose()
        {
        }
    }
}