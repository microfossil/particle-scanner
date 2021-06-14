using Emgu.CV;
using ImageProcessing.Classification.Tensorflow;

namespace ImageProcessing.Images
{
    public class ProcessedMat
    {
        public Mat Mat;
        public Morphology.Morphology Morphology { get; set; }
        public Probability Probability { get; set; }

        public ProcessedMat()
        {
            Morphology = new Morphology.Morphology();
        }

        public ProcessedMat(Mat mat) : this()
        {
            Mat = mat;
        }

        public void Release()
        {
            Mat.Dispose();
        }
    }
}
