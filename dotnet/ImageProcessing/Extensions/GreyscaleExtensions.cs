using Emgu.CV;
using Emgu.CV.CvEnum;

namespace ImageProcessing.Extensions
{
    public static class GreyscaleExtensions
    {
        public static Mat CreateGreyscaleRedGreen(this Mat mat)
        {
            Mat output = new Mat();
            mat.ConvertTo(output, DepthType.Cv32F, 0.5, 0);
            Mat red = new Mat();
            Mat green = new Mat();
            CvInvoke.ExtractChannel(output, red, 2);
            CvInvoke.ExtractChannel(output, green, 1);
            CvInvoke.Add(red, green, output);
            red.Dispose();
            green.Dispose();
            return output;
        }

        public static Mat CreateGreyscaleRedGreen8U(this Mat mat)
        {
            Mat output = mat.CreateGreyscaleRedGreen();
            output.ConvertTo(output, DepthType.Cv8U);
            return output;
        }

        public static Mat SumChannels(this Mat mat)
        {
            Mat output = new Mat();
            mat.ConvertTo(output, DepthType.Cv32F, 1.0/3, 0);
            Mat red = new Mat();
            Mat green = new Mat();
            Mat blue = new Mat();
            CvInvoke.ExtractChannel(output, red, 2);
            CvInvoke.ExtractChannel(output, green, 1);
            CvInvoke.ExtractChannel(output, blue, 0);
            CvInvoke.Add(red, green, output);
            CvInvoke.Add(output, blue, output);
            red.Dispose();
            green.Dispose();
            blue.Dispose();
            return output;
        }

        public static Mat SumChannels8U(this Mat mat)
        {
            Mat output = mat.SumChannels();
            output.ConvertTo(output, DepthType.Cv8U);
            return output;
        }
    }
}
