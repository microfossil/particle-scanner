using Emgu.CV;
using ImageProcessing.Images;

namespace ImageProcessing.Stacking
{
    public interface IStacker
    {
        void Add(Mat image, bool isNewStack);
        StackedMat Stack();
        void Start();
        void Stop();        
    }
}
