using System.Windows.Media.Imaging;

namespace ImageProcessing.Images
{
    public class LabeledWriteableBitmap
    {
        public WriteableBitmap Image { get; set; }
        public string Title { get; set; }
        public string Label { get; set; }

        public LabeledWriteableBitmap(WriteableBitmap image, string title)
        {
            this.Image = image;
            this.Title = title;
            this.Label = "";
        }

        public LabeledWriteableBitmap(WriteableBitmap image, string title, string label)
        {
            this.Image = image;
            this.Title = title;
            this.Label = label;
        }
    }
}
