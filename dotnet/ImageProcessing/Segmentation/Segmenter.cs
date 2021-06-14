using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using ImageProcessing.Extensions;
using ImageProcessing.Images;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace ImageProcessing.Segmentation
{
    public class Segmenter
    {
        public static readonly double[] NORM_INPUT_PROB = { 0.5, 0.98 };
        public static readonly double[] NORM_OUTPUT_RANGE = { 0.7, 0.95 };
        public const int DILATION_RADIUS = 3;
        public const double GAUSSIAN_SIGMA = 1.5;
        public const double BORDER_BUFFER = 1.1;

    
        public static bool CheckForParticle(Mat mat, Mat diff, int threshold, int areaThreshold, double convexityThreshold, int buffer)
        {
            var contours = FindBinaryContours(diff, threshold);
            RectangleF validArea = new RectangleF(buffer, buffer, mat.Width - 2 * buffer, mat.Height - 2 * buffer);
            (var valid, var invalid) = FindValidContours(contours, areaThreshold, convexityThreshold, validArea);
            return valid.Count > 0;
        }

        public static (List<Contour>, List<Contour>) FindValidContours(List<Contour> contours, int areaThreshold, double convexityThreshold, RectangleF validArea)
        {
            var valid = contours.Where(x => x.Area >= areaThreshold &&
                                       x.ConvexityRatio >= convexityThreshold &&
                                       validArea.Contains(x.Centroid)).ToList();
            var invalid = contours.Where(x => x.Area < areaThreshold ||
                                              x.ConvexityRatio < convexityThreshold ||
                                              !validArea.Contains(x.Centroid)).ToList();
            return (valid, invalid);
        }

        public static (List<MaskedMat>, List<StackedMat>, List<Contour>) Segment(
            StackedMat stackedMat,
            Mat differenceImage,
            int threshold,
            int areaThreshold,
            double convexityThreshold,
            double cropBuffer,
            int searchBuffer = 0,
            int openingRadius = 0)
        {
            //If we are not using a difference image, just clone the greyscale image
            Mat diff;
            if (differenceImage != null)
            {
                diff = differenceImage.Clone();
            }
            else
            {
                diff = stackedMat.Greyscale.Clone();
            }

            //Find the contours
            List<Contour> contours;
            if (openingRadius > 0)
            {
                contours = FindBinaryContoursOpening(diff, threshold, openingRadius);
            }
            else
            {
                contours = FindBinaryContours(diff, threshold);
            }
            diff.Dispose();

            //Only use values within the valid area of the image
            RectangleF validArea = new RectangleF(searchBuffer, searchBuffer, stackedMat.Colour.Width - 2 * searchBuffer, stackedMat.Colour.Height - 2 * searchBuffer);
            (var validContours, var invalidContours) = FindValidContours(contours, areaThreshold, convexityThreshold, validArea);

            //ReceptacleData mask
            Mat stackedMask = new Mat(stackedMat.Colour.Rows, stackedMat.Colour.Cols, DepthType.Cv8U, 1);
            stackedMask.SetTo(new MCvScalar(0));
            stackedMat.Mask = stackedMask;
            
            //Segment the image defined by each contour
            Mat mat = stackedMat.Colour;
            List<MaskedMat> crops = new List<MaskedMat>();
            List<StackedMat> stackedCrops = new List<StackedMat>();
            foreach (Contour contour in validContours)
            {
                //Get max radius
                double maxRadius = 0;
                for (int i = 0; i < contour.Points.Size; i++)
                {
                    double radius = Math.Sqrt(Math.Pow(contour.Points[i].X - contour.Centroid.X, 2) + Math.Pow(contour.Points[i].Y - contour.Centroid.Y, 2));
                    if (radius > maxRadius)
                    {
                        maxRadius = radius;
                    }
                }

                //Crop area
                int halfWidth = (int)Math.Round(maxRadius * cropBuffer);
                SquareWithinBounds sq = new SquareWithinBounds(contour.Centroid, halfWidth, mat.Size);

                //Crop
                var cropArea = sq.Rectangle;
                Mat cropTemp = new Mat(mat, cropArea);
                Mat crop = new Mat();
                cropTemp.CopyTo(crop);
                cropTemp.Dispose();

                //Raw mask
                Mat mask = new Mat(mat.Rows, mat.Cols, DepthType.Cv8U, 1);
                mask.SetTo(new MCvScalar(0));
                VectorOfVectorOfPoint contourArray = new VectorOfVectorOfPoint(contour.Points.Clone());
                CvInvoke.FillPoly(mask, contourArray, new MCvScalar(255));
                CvInvoke.FillPoly(stackedMat.Mask, contourArray, new MCvScalar(255));
                Mat maskTemp = new Mat(mask, cropArea);
                mask = new Mat();
                maskTemp.CopyTo(mask);
                maskTemp.Dispose();

                //1-pixel closing
                Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(5, 5), new Point(-1, -1));
                CvInvoke.MorphologyEx(mask, mask, MorphOp.Open, kernel, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0.0));
                CvInvoke.MorphologyEx(stackedMat.Mask, stackedMat.Mask, MorphOp.Open, kernel, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0.0));

                //Shift contour
                var shiftedContour = contour.Points.Translate((Size)Point.Subtract(sq.Centroid, (Size) contour.Centroid));
                var maskedMat = new MaskedMat(crop, mask, shiftedContour, sq.Centroid);
                maskedMat.Area = contour.Area;
                crops.Add(maskedMat);

                //Stacked Mat
                StackedMat stackedCrop = stackedMat.CreateCrop(cropArea);
                stackedCrop.Mask = mask.Clone();
                stackedCrops.Add(stackedCrop);
            }
            return (crops, stackedCrops, validContours);
        }


        //TODO update to latest
        public static List<ProcessedMat> ProcessAndCalculateMorphology(
            List<MaskedMat> mats,
            double threshold,
            double buffer,
            double pixelAreaMM,
            bool rotate = false,
            bool applyNormalisation = false,
            double normalisationFactor = 0.7,
            bool removeBackground = false,
            int backgroundBuffer = 16)
        {
            List<ProcessedMat> processedMats = new List<ProcessedMat>();
            foreach (MaskedMat maskedMat in mats)
            {
                Mat mat = maskedMat.Mat;
                Mat mask = maskedMat.Mask;

                //Convert to greyscale
                Mat gMat;
                //if (mat.NumberOfChannels > 1)
                //{
                //    gMat = mat.CreateGreyscaleRedGreen8U();
                //}
                //else
                //{
                    gMat = mat.Clone();
                //}

                if (false)
                {
                    

                    //Ellipse
                    Moments moments = CvInvoke.Moments(mask);
                    PointF centroid = moments.CentroidF();
                    var (maxRadius, maxAngle) = maskedMat.MaskContour.MaxRadius(centroid);
                    double orientation = 0;
                    //if (rotate)
                    //{
                    //    orientation = Math.Cos(maxAngle) < 0 ? moments.EllipseOrientation() : (moments.EllipseOrientation() + Math.PI) % Math.PI;
                    //}

                    //1-pixel closing
                    Mat kernel =
                        CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(3, 3), new Point(-1, -1));
                    CvInvoke.MorphologyEx(mask, mask, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Constant,
                        new MCvScalar(0.0));

                    //Normalise
                    gMat.ConvertTo(gMat, DepthType.Cv32F);
                    mask.ConvertTo(mask, DepthType.Cv32F, 1.0 / 255);
                    //if (applyNormalisation)
                    //{
                    //    gMat.NormaliseFromDistribution(mask, NORM_INPUT_PROB, new double[] { normalisationFactor, NORM_OUTPUT_RANGE[1] });
                    //}
                    //else
                    //{
                    gMat.Multiply(1.0 / 255.0);
                    //}

                    //Dilate and smooth          
                    //if (removeBackground)
                    //{
                    //    Mat se = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(backgroundBuffer * 2 + 1, backgroundBuffer * 2 + 1), new Point(-1, -1));
                    //    CvInvoke.Dilate(mask, mask, se, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0.0));
                    //    CvInvoke.GaussianBlur(mask, mask, new Size(0, 0), backgroundBuffer / 2);

                    //    //Apply mask  
                    //    CvInvoke.Multiply(gMat, mask, gMat);
                    //}

                    //Rotate and centre
                    var finalWidth = (int) Math.Round(maxRadius * 2 * buffer);
                    var finalSize = new Size(finalWidth, finalWidth);
                    var halfWidth = (float) finalWidth / 2;

                    if (!rotate) orientation = 0;
                    Mat transform =
                        GetTranslationRotationScaleMatrix(new PointF(halfWidth, halfWidth), centroid, -orientation, 1);
                    CvInvoke.WarpAffine(gMat, gMat, transform, finalSize, borderMode: BorderType.Reflect101);

                    //Convert and save
                    
                }
                gMat.ConvertTo(gMat, DepthType.Cv32F);
                gMat.Multiply(1.0/255.0);

                int maskArea = (int) CvInvoke.Sum(mask).V0 / 255;

                //gMat.Multiply(255.0);
                //gMat.ConvertTo(gMat, DepthType.Cv8U);
                ProcessedMat pi = new ProcessedMat(gMat);
                pi.Morphology.Area = maskArea;
                pi.Morphology.AreaMM = maskArea * pixelAreaMM;
                pi.Morphology.MeanDiameterMM = Math.Sqrt(pi.Morphology.AreaMM / Math.PI) * 2;
                processedMats.Add(pi);
            }
            return processedMats;
        }

        public static List<ProcessedMat> PreprocessRGB(List<MaskedMat> mats, double threshold, int areaThreshold, double buffer, double pixelAreaMM)
        {
            List<ProcessedMat> processedMats = new List<ProcessedMat>();
            foreach (MaskedMat maskedMat in mats)
            {
                Mat mat = maskedMat.Mat;
                Mat mask = maskedMat.Mask;

                Mat gMat = mat.Clone();

                int maskArea = (int)CvInvoke.Sum(mask).V0;

                //Ellipse
                Moments moments = CvInvoke.Moments(mask);
                PointF centroid = moments.CentroidF();
                var (maxRadius, maxAngle) = maskedMat.MaskContour.MaxRadius(centroid);
                double orientation = Math.Cos(maxAngle) < 0 ? moments.EllipseOrientation() : (moments.EllipseOrientation() + Math.PI) % Math.PI;

                //1-pixel closing
                Mat kernel = new Mat(3, 3, DepthType.Cv8U, 1);
                kernel.SetTo(new MCvScalar(1.0));
                CvInvoke.MorphologyEx(mask, mask, MorphOp.Close, kernel, new Point(1, 1), 1, BorderType.Constant, new MCvScalar(0.0));

                //Normalise
                gMat.ConvertTo(gMat, DepthType.Cv32F);
                //NormaliseFromDistribution(gMat, mask, NORM_INPUT_PROB, NORM_OUTPUT_RANGE);

                //Dilate and smooth
                mask.ConvertTo(mask, DepthType.Cv32F, 1.0 / 255);
                Mat se = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(DILATION_RADIUS * 2 + 1, DILATION_RADIUS * 2 + 1), new Point(-1, -1));
                CvInvoke.Dilate(mask, mask, se, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0.0));
                CvInvoke.GaussianBlur(mask, mask, new Size(0, 0), GAUSSIAN_SIGMA);

                //Apply mask  
                Mat[] gMats = gMat.Split();
                CvInvoke.Multiply(gMats[0], mask, gMats[0]);
                CvInvoke.Multiply(gMats[1], mask, gMats[1]);
                CvInvoke.Multiply(gMats[2], mask, gMats[2]);
                CvInvoke.Merge(new VectorOfMat(gMats), gMat);

                //Rotate and centre
                var finalWidth = (int)Math.Round(maxRadius * 2 * BORDER_BUFFER);
                var finalSize = new Size(finalWidth, finalWidth);
                var halfWidth = (float)finalWidth / 2;
                Mat transform = GetTranslationRotationScaleMatrix(new PointF(halfWidth, halfWidth), centroid, -orientation, 1);
                CvInvoke.WarpAffine(gMat, gMat, transform, finalSize);

                //Convert and save
                gMat.ConvertTo(gMat, DepthType.Cv8U);
                ProcessedMat pi = new ProcessedMat(gMat);
                pi.Morphology.Area = maskArea;
                pi.Morphology.AreaMM = maskArea * pixelAreaMM;
                processedMats.Add(pi);
            }
            return processedMats;
        }

        public static void DrawSegments(Mat mat, Mat diff, int threshold, int areaThreshold, double convexityThreshold, int buffer, bool showLines, int openingRadius = 0)
        {
            List<Contour> contours;
            if (openingRadius > 0)
            {
                contours = FindBinaryContoursOpening(diff, threshold, openingRadius);
            }
            else
            {
                contours = FindBinaryContours(diff, threshold);
            }
            RectangleF validArea = new RectangleF(buffer, buffer, diff.Width - 2 * buffer, diff.Height - 2 * buffer);

            var validContours = contours.Where(x => x.Area >= areaThreshold &&
                                            x.ConvexityRatio >= convexityThreshold &&
                                            validArea.Contains(x.Centroid)).ToList();
            var outsideContours = contours.Where(x => x.Area >= areaThreshold &&
                                                    x.ConvexityRatio >= convexityThreshold &&
                                                    !validArea.Contains(x.Centroid)).ToList();
            var invalidContours = contours.Where(x => x.Area < areaThreshold ||
                                                      x.ConvexityRatio < convexityThreshold).ToList();

            var width = mat.Width / 20;

            if (showLines)
            {
                foreach (var contour in validContours)
                {
                    DrawCrosshairs(ref mat, contour.Centroid.X, contour.Centroid.Y, width / 5, width / 10, new Bgr(System.Drawing.Color.MediumSpringGreen));
                }
                foreach (var contour in outsideContours)
                {
                    DrawCrosshairs(ref mat, contour.Centroid.X, contour.Centroid.Y, width / 5, width / 10, new Bgr(System.Drawing.Color.Red));
                }
                if (validContours.Count > 0)
                {
                    VectorOfVectorOfPoint v = new VectorOfVectorOfPoint(validContours.Select(x => x.Points).ToArray());
                    CvInvoke.DrawContours(mat, v, -1, new Bgr(System.Drawing.Color.MediumSpringGreen).MCvScalar, width / 15);
                }
                if (outsideContours.Count > 0)
                {
                    VectorOfVectorOfPoint v = new VectorOfVectorOfPoint(outsideContours.Select(x => x.Points).ToArray());
                    CvInvoke.DrawContours(mat, v, -1, new Bgr(System.Drawing.Color.Red).MCvScalar, width / 15);
                }
                if (invalidContours.Count > 0)
                {
                    VectorOfVectorOfPoint v = new VectorOfVectorOfPoint(invalidContours.Select(x => x.Points).ToArray());
                    CvInvoke.DrawContours(mat, v, -1, new Bgr(System.Drawing.Color.MediumVioletRed).MCvScalar, width / 40);
                }
                if (buffer > 0)
                {
                    DrawRectangle(ref mat, buffer, buffer, mat.Width-2*buffer, mat.Height-2*buffer, width / 10, new Bgr(System.Drawing.Color.LightBlue));
                }
                DrawRectangle(ref mat, (int)30, (int)30, (int)(Math.Sqrt(areaThreshold / Math.PI) * 2), (int)(1), 10, new Bgr(System.Drawing.Color.MediumVioletRed));
            }
        }

        public static List<Contour> FindBinaryContours(Mat mat, double threshold)
        {
            var diff = mat.Clone();
            CvInvoke.Threshold(diff, diff, threshold, 255, ThresholdType.Binary);

            //Mat kernel = new Mat(3, 3, DepthType.Cv8U, 1);
            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(7, 7), new Point(-1, -1));
            kernel.SetTo(new MCvScalar(1.0));
            CvInvoke.MorphologyEx(diff, diff, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0.0));

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(diff, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            diff.Dispose();

            return ConvertToContours(contours);
        }

        public static List<Contour> ConvertToContours(VectorOfVectorOfPoint contours)
        {
            List<Contour> convertedContours = new List<Contour>();
            for (int i = 0; i < contours.Size; i++)
            {
                convertedContours.Add(new Contour(contours[i]));
            }
            return convertedContours;
        }

        public static List<Contour> FindBinaryContoursOpening(Mat mat, double threshold, int radius)
        {
            var diff = mat.Clone();
            CvInvoke.Threshold(diff, diff, threshold, 255, ThresholdType.Binary);

            //Mat kernel = new Mat(3, 3, DepthType.Cv8U, 1);
            Mat kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(radius, radius), new Point(-1, -1));
            kernel.SetTo(new MCvScalar(1.0));
            CvInvoke.MorphologyEx(diff, diff, MorphOp.Open, kernel, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0.0));

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(diff, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            diff.Dispose();

            return ConvertToContours(contours);
        }










        public static Mat TranslationMatrix(PointF offset)
        {
            float[] T = new float[6] { 1, 0, -offset.X, 0, 1, -offset.Y };

            Mat Tmat = Mat.Zeros(2, 3, DepthType.Cv32F, 1);
            Marshal.Copy(T, 0, Tmat.DataPointer, 6);

            return Tmat;
        }

        public static Mat GetTranslationRotationScaleMatrix(PointF imageCentre, PointF rotationCentre, double rotation, double scale)
        {
            float cosTheta = (float)Math.Cos(rotation);
            float sinTheta = (float)Math.Sin(rotation);

            float[] C = new float[9] { 1, 0, imageCentre.X, 0, 1, imageCentre.Y, 0, 0, 1 };
            float[] T = new float[9] { 1, 0, -rotationCentre.X, 0, 1, -rotationCentre.Y, 0, 0, 1 };
            float[] R = new float[9] { cosTheta, -sinTheta, 0, sinTheta, cosTheta, 0, 0, 0, 1 };
            float[] S = new float[9] { (float)scale, 0, 0, 0, (float)scale, 0, 0, 0, 1 };

            Mat Tmat = Mat.Zeros(3, 3, DepthType.Cv32F, 1);
            Marshal.Copy(T, 0, Tmat.DataPointer, 9);
            Mat Rmat = Mat.Zeros(3, 3, DepthType.Cv32F, 1);
            Marshal.Copy(R, 0, Rmat.DataPointer, 9);
            Mat Smat = Mat.Zeros(3, 3, DepthType.Cv32F, 1);
            Marshal.Copy(S, 0, Smat.DataPointer, 9);
            Mat Cmat = Mat.Zeros(3, 3, DepthType.Cv32F, 1);
            Marshal.Copy(C, 0, Cmat.DataPointer, 9);

            Mat result = Mat.Zeros(3, 3, DepthType.Cv32F, 1);
            CvInvoke.Gemm(Cmat, Smat, 1, null, 0, result);
            CvInvoke.Gemm(result, Rmat, 1, null, 0, result);
            CvInvoke.Gemm(result, Tmat, 1, null, 0, result);
            return new Mat(result, new Rectangle(0, 0, 3, 2));
        }

        private static void DrawCrosshairs(ref Mat mat, int x, int y, int size, int thickness, Bgr color)
        {
            CvInvoke.Line(mat, new System.Drawing.Point(x, y - size), new System.Drawing.Point(x, y + size), color.MCvScalar, thickness);
            CvInvoke.Line(mat, new System.Drawing.Point(x - size, y), new System.Drawing.Point(x + size, y), color.MCvScalar, thickness);
        }

        private static void DrawRectangle(ref Mat mat, int x, int y, int width, int height, int thickness, Bgr color)
        {
            CvInvoke.Rectangle(mat, new Rectangle(x, y, width, height), color.MCvScalar, thickness);
        }
    }
}
