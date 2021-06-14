using System.Windows.Media.Imaging;
using Emgu.CV;
using ImageProcessing.Classification.Tensorflow;
using ImageProcessing.Extensions;

namespace ImageProcessing.Images
{
    public class ProcessedWriteableBitmap
    {
        //Image
        public WriteableBitmap Image { get; set; }
        public string Label { get; set; }
        public string Title { get; set; }
        public Morphology.Morphology Morphology { get; set; }
        public Probability Probability { get; set; }

        public ProcessedWriteableBitmap()
        {
            Morphology = new Morphology.Morphology();
        }

        public ProcessedWriteableBitmap(ProcessedMat mat, double scale) : this()
        {
            Image = mat.Mat.CreateWriteableBitmap(scale);
            Morphology = mat.Morphology;
            Probability = mat.Probability;
        }

        public ProcessedWriteableBitmap(Mat mat, double scale) : this()
        {
            this.Image = mat.CreateWriteableBitmap(scale);
        }

        public ProcessedWriteableBitmap(Mat mat, string title, double scale) : this()
        {
            this.Image = mat.CreateWriteableBitmap(scale);
            this.Title = title;
            this.Label = "";
        }

        public ProcessedWriteableBitmap(WriteableBitmap image, string title) : this()
        {
            this.Image = image;
            this.Title = title;
            this.Label = "";
        }

        public ProcessedWriteableBitmap(WriteableBitmap image, string title, string label) : this()
        {
            this.Image = image;
            this.Title = title;
            this.Label = label;
        }
    }
}
