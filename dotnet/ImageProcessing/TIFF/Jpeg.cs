using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ImageProcessing.TIFF
{
    public class Jpeg
    {
        public byte[] Data;
        public uint Width;
        public uint Height;
        public uint HorizontalResolution;
        public uint VerticalResolution;

        public Jpeg(byte[] data, uint width, uint height, uint horizontalResolution, uint verticalResolution)
        {
            this.Data = data;
            this.Width = width;
            this.Height = height;
            this.HorizontalResolution = horizontalResolution;
            this.VerticalResolution = verticalResolution;
        }

        public static Jpeg FromBitmapFrame(BitmapFrame bitmap, long quality)
        {
            using (var stream = new MemoryStream())
            {
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 90;
                encoder.Frames.Add(bitmap);
                encoder.Save(stream);
                return new Jpeg(stream.ToArray(), (uint)bitmap.Width, (uint)bitmap.Height, (uint)bitmap.DpiX, (uint)bitmap.DpiY);
            }
        }

        public static Jpeg FromBitmap(Bitmap bitmap, long quality)
        {
            using (var stream = new MemoryStream())
            {
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                Encoder encoder = Encoder.Quality;
                EncoderParameters parameters = new EncoderParameters(1);
                parameters.Param[0] = new EncoderParameter(encoder, quality);
                bitmap.Save(stream, jpgEncoder, parameters);
                return new Jpeg(stream.ToArray(), (uint) bitmap.Width, (uint) bitmap.Height, (uint) bitmap.HorizontalResolution, (uint) bitmap.VerticalResolution);
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            return ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);
        }
    }
}
