using System;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace ImageProcessing.Extensions
{
    public static class NormalisationExtensions
    {
        public static void NormaliseFromDistribution(this Mat mat, Mat mask, double[] prob, double[] range)
        {
            //Find pixel intensities corresponding to probability.
            Mat norm = mat.Clone();
            CvInvoke.Multiply(norm, mask, norm, dtype: DepthType.Cv32F);
            int len = norm.Width * norm.Height;
            var array = norm.GetData();
            var buffer = new float[len];
            Buffer.BlockCopy(array, 0, buffer, 0, array.Length * sizeof(float));
            //Sort into order
            Array.Sort(buffer);
            //First non-zero (mask) element (perhaps change to > 0.01)
            int start = Array.LastIndexOf(buffer, (float)0.0);
            int d = len - start;
            //Index
            int probStartIdx = (int)Math.Round((double)d * prob[0]) + start;
            int probEndIdx = (int)Math.Round((double)d * prob[1]) + start;
            //Values
            double min = buffer[probStartIdx];
            double max = buffer[probEndIdx];

            // norm = greyscaleImage.clone();
            //Core.divide(norm, new Scalar(255), norm);
            //int diff = size - start;
            //int topIdx = (int)Math.round(diff * 0.98) + start;
            //int midIdx = (int)Math.round(diff * 0.5) + start;

            //float topVal = patch[topIdx];
            //float midVal = patch[midIdx];

            var mult = range[1] / max;
            mat.Multiply(mult);
            var pwr = Math.Log(range[0]) / Math.Log(min * mult);
            if (pwr < 1.0) pwr = 1.0;
            CvInvoke.Pow(mat, pwr, mat);

            Debug.WriteLine($"Normalisation {range[0]} {mult}  {pwr}");

            //Core.multiply(norm, new Scalar(mult), norm);

            //float pwr = (float)(Math.log(contrast) / Math.log(midVal * mult));
            //Core.pow(norm, pwr, norm);

            //Core.multiply(norm, new Scalar(255), workingImage);
            //norm.release();

            //Normalise
            //var offrange = range[1] - range[0];
            //var normrange = max - min;
            //mat.Add(-min);
            //mat.Multiply(offrange / normrange);
            //mat.Add(range[0]);
            //Clamp to [0,1]
            CvInvoke.Threshold(mat, mat, 0, 1, ThresholdType.ToZero);
            CvInvoke.Threshold(mat, mat, 1, 1, ThresholdType.Trunc);
        }

        public static void NormaliseFromHistogram(this Mat mat, Mat mask)
        {
            CvInvoke.EqualizeHist(mat, mat);
        }

        public static void NormaliseMaximum(this Mat mat, Mat mask)
        {
            var (min, max) = mat.MinMax(mask);
            mat.Multiply(1.0 / max);
            CvInvoke.Threshold(mat, mat, 0, 1, ThresholdType.ToZero);
            CvInvoke.Threshold(mat, mat, 1, 1, ThresholdType.Trunc);
        }

        public static double ValueAtProbability(this Mat mat, Mat mask, double prob)
        {
            //Find pixel intensities corresponding to probability.
            Mat norm = mat.Clone();
            if (mask != null)
            {
                CvInvoke.Multiply(norm, mask, norm, dtype: DepthType.Cv32F);
            }
            else
            {
                norm.ConvertTo(norm, DepthType.Cv32F);
            }
            int len = norm.Width * norm.Height;
            var array = norm.GetData();
            var buffer = new float[len];
            Buffer.BlockCopy(array, 0, buffer, 0, array.Length * sizeof(float));
            //Sort into order
            Array.Sort(buffer);
            //First non-zero (mask) element
            int start = 0;
            if (mask != null)
            {
                start = Array.LastIndexOf(buffer, (float)0.0);
            }
            int d = len - start;
            //Index
            int idx = (int)Math.Round((double)d * prob) + start;
            //Values
            double value = buffer[idx];
            norm.Dispose();

            return value;
        }
    }
}
