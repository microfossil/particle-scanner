using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using ImageProcessing.Extensions;
using ImageProcessing.Segmentation;

namespace ImageProcessing.Background
{
    public class ColourBackgroundModel
    {
        /*
         * Median Background Modeller
         *
         * A stack is a single image with N channels, usually around 9
         * Each `Update` inserts a new greyscale image (using `Process`) into a channel of the stack according to the current index
         * The index is incremented and if it is greater than the size of the stack it is reset to zero
         * The stack is thus a circular buffer of the last N background image estimates
         *
         * When Calculate is called, each pixel in the stack is sorted along the channel dimension
         * We can then take a slice of the stack at a certain index, for example for N = 9, taking the 5th slice gives the median
         * This slice becomes the background model
         *
         * The code supports multiple background models (the `idx` parameter in some methods)
         */

        public int Id { get; private set; }

        //Width and Height
        private int width = 0;
        private int height = 0;

        //Input stacks
        //Each stack has number of channels equal to the number of images considered in the model
        private Mat greyStack;
        //private Mat redStack;
        //private Mat greenStack;
        //private Mat blueStack;

        private List<Mat> rgbMats;

        //Sorted stacks
        //The input stacks sorted by value 
        private Mat sortedGreyStack;
        //private Mat sortedRedStack;
        //private Mat sortedGreenStack;
        //private Mat sortedBlueStack;

        //Current index in the stack
        private int currentIdx;

        //Has the model been initialised
        private bool wasFirstImageReceived;

        //Final models
        public Mat GreyscaleModel { get; private set; }
        public Mat ColourModel { get; private set; }

        //Offset monitor
        private const int MAXIMUM_OFFSET_BEFORE_RESET = 150;
        private const int MAXIMUM_COUNT_BEFORE_RESET = 2;
        private int badOffsetCount;

        //Event for when the model is updated
        public event Action<ColourBackgroundModel> ModelUpdatedEvent = delegate { };

        //Model parameters

        //The initialThreshold is used for two things:
        // - initialising the background models
        // - masking out any bright regions of the input images when aligning images with model
        // - as a comparison for the first difference measurement
        private int initialThreshold;

        //The number of images in the stack
        private int stackSize;
        public int StackSize
        {
            get => stackSize;
            set
            {
                stackSize = value;
                OnStackSizeChanged();
            }
        }

        //The position in the sorted stack to use for the model
        private int selectionIndex;
        public int SelectionIndex
        {
            get => selectionIndex;
            set
            {
                selectionIndex = value;
                OnSelectedIndexChanged();
            }
        }

        /// <summary>
        /// Construct the colour background model
        /// </summary>
        /// <param name="width">Image Width</param>
        /// <param name="height">Image Height</param>
        /// <param name="stackSize">Number of images to use in the stack</param>
        /// <param name="selectionIndex">Position in the sorted stack of image to use as the model</param>
        /// <param name="initialThreshold">The initial model value (choose something between the background and the particle brightness)</param>
        public ColourBackgroundModel(int id, int width, int height, int stackSize, int selectionIndex, int initialThreshold)
        {
            this.Id = id;
            if (selectionIndex >= stackSize) throw new ArgumentOutOfRangeException(nameof(selectionIndex), @"Selection index must be less than stack size");
            this.width = width;
            this.height = height;
            this.stackSize = stackSize;
            this.selectionIndex = selectionIndex;
            this.initialThreshold = initialThreshold;

            rgbMats = new List<Mat>();

            Initialise();
        }

        /// <summary>
        /// Initialising the model
        /// </summary>
        public void Initialise()
        {
            greyStack?.Dispose();
            //redStack?.Dispose();
            //blueStack?.Dispose();
            //greenStack?.Dispose();

            sortedGreyStack?.Dispose();
            //sortedRedStack?.Dispose();
            //sortedBlueStack?.Dispose();
            //sortedGreenStack?.Dispose();

            GreyscaleModel?.Dispose();
            ColourModel?.Dispose();

            //Initialising the stacks
            greyStack = createMultichannelMat(width, height, stackSize, DepthType.Cv8U, initialThreshold);
            //redStack = createMultichannelMat(Width, Height, stackSize, DepthType.Cv8U, initialThreshold);
            //blueStack = createMultichannelMat(Width, Height, stackSize, DepthType.Cv8U, initialThreshold);
            //greenStack = createMultichannelMat(Width, Height, stackSize, DepthType.Cv8U, initialThreshold);

            sortedGreyStack = createMultichannelMat(width, height, stackSize, DepthType.Cv8U, initialThreshold);
            //sortedRedStack = createMultichannelMat(Width, Height, stackSize, DepthType.Cv8U, initialThreshold);
            //sortedBlueStack = createMultichannelMat(Width, Height, stackSize, DepthType.Cv8U, initialThreshold);
            //sortedGreenStack = createMultichannelMat(Width, Height, stackSize, DepthType.Cv8U, initialThreshold);

            foreach (var rgbMat in rgbMats)
            {
                rgbMat.Dispose();
            }
            rgbMats.Clear();
            for (int i = 0; i < stackSize; i++)
            {
                rgbMats.Add(createMultichannelMat(width, height, 3, DepthType.Cv8U, initialThreshold));
            }

            //Initialising the models
            GreyscaleModel = createMultichannelMat(width, height, 1, DepthType.Cv8U, initialThreshold);
            ColourModel = createMultichannelMat(width, height, 3, DepthType.Cv8U, initialThreshold);

            //Other
            badOffsetCount = 0;
            currentIdx = 0;
            wasFirstImageReceived = false;

            ModelUpdatedEvent(this);
        }

        /// <summary>
        /// Creates a mat with an abitrary number of channels (not supported by default by OpenCV)
        /// </summary>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="channels">Number of channels</param>
        /// <param name="depthType">Type of image</param>
        /// <param name="initialValue">Initial value of all channels</param>
        /// <returns></returns>
        private Mat createMultichannelMat(int width, int height, int channels, DepthType depthType, int initialValue)
        {
            var mat = new Mat(height, width * channels, depthType, 1);
            mat.SetTo(new MCvScalar(initialValue));
            mat = mat.Reshape(channels);
            return mat;
        }

        /// <summary>
        /// Calculate the GREYSCALE difference between the input image and the background model
        /// </summary>
        /// <param name="mat">Input image</param>
        /// <returns>Difference image</returns>
        public (Mat, PointF) Difference(Mat mat)
        {
            //Convert the input image to greyscale
            Mat greyscale = Process(mat);
            PointF offset = new PointF(0, 0);

            if (!wasFirstImageReceived)
            {
                //Compare against the mean of the pixel values outside of the particle
                //The boundaries of the particle are determined using the initialThreshold
                using (var mask = new Mat())
                {
                    CvInvoke.Threshold(greyscale, mask, initialThreshold, 255, ThresholdType.BinaryInv);
                    MCvScalar mean = new MCvScalar();
                    MCvScalar std = new MCvScalar();
                    CvInvoke.MeanStdDev(greyscale, ref mean, ref std);
                    greyscale.Subtract(mean.V0);
                }
            }
            else
            {
                //Calculate background offset with input image with brightness removed
                offset = CalculateOffset(greyscale);

                using (var bkg = GreyscaleModel.Clone())
                {
                    //Shift the background
                    bkg.Translate(offset, BorderType.Reflect101);
                    //Calculate the difference
                    CvInvoke.Subtract(greyscale, bkg, greyscale);
                }
            }
            return (greyscale, offset);
        }

        /// <summary>
        /// Calculate the COLOUR difference between the input image and the background model
        /// </summary>
        /// <param name="mat">Input image</param>
        /// <returns></returns>
        public (Mat, PointF) ComputeColourDifference(Mat mat)
        {
            var greyscale = Process(mat);
            var rgb = mat.Clone();
            PointF offset = new PointF(0, 0);

            if (!wasFirstImageReceived)
            {
                //Compare against the mean of the pixel values outside of the particle
                //The boundaries of the particle are determined using the initialThreshold
                using (var mask = new Mat())
                {
                    CvInvoke.Threshold(greyscale, mask, initialThreshold, 255, ThresholdType.BinaryInv);
                    MCvScalar mean = new MCvScalar();
                    MCvScalar std = new MCvScalar();
                    CvInvoke.MeanStdDev(rgb, ref mean, ref std);
                    rgb.Subtract(mean.V0, mean.V1, mean.V2);
                    greyscale = rgb.SumChannels8U();
                }
            }
            else
            {
                //Calculate background offset with input image with brightness removed
                offset = CalculateOffset(greyscale);

                using (var bkg = ColourModel.Clone())
                {
                    //Shift the background
                    bkg.Translate(offset, BorderType.Reflect101);
                    //Calculate the difference
                    CvInvoke.Subtract(rgb, bkg, rgb);
                    greyscale = rgb.SumChannels8U();
                }
            }
            rgb.Dispose();
            return (greyscale, offset);
        }


        /// <summary>
        /// Calculate the offset between the background model and the greyscale input image
        /// </summary>
        /// <param name="greyscale">Input image</param>
        /// <param name="mask">Mask of regions to remove (i.e. particles)</param>
        /// <returns></returns>
        public PointF CalculateOffset(Mat greyscale, Mat mask = null)
        {
            PointF offset;
            if (mask == null)
            {
                using (Mat brightRemoved = greyscale.Clone())
                {
                    CvInvoke.Threshold(brightRemoved, brightRemoved, initialThreshold, initialThreshold,
                        ThresholdType.Trunc);
                    brightRemoved.ConvertTo(brightRemoved, DepthType.Cv32F);
                    offset = GreyscaleModel.CalculatePhaseCorrection(brightRemoved);
                }
            }
            else
            {
                using (Mat brightRemoved = greyscale.Clone())
                {
                    brightRemoved.SetTo(new MCvScalar(initialThreshold), mask);
                    brightRemoved.ConvertTo(brightRemoved, DepthType.Cv32F);
                    offset = GreyscaleModel.CalculatePhaseCorrection(brightRemoved);
                }
            }
            return offset;
        }
        
        /// <summary>
        /// Update the background model with the input image
        /// If we have had multiple large offsets in a row, the model is reset
        /// </summary>
        /// <param name="mat">Input image</param>
        /// <param name="contours">Contours of regions to ignore</param>
        /// <param name="offset">Offset between the image and the background (null to calculate in place)</param>
        public void Update(Mat mat, List<Contour> contours, PointF? offsetNullable = null)
        {
            //Double check we are not out of bounds
            if (offsetNullable != null)
            {
                var offset = offsetNullable.Value;
                var offsetM = Math.Sqrt(Math.Pow(offset.X, 2) + Math.Pow(offset.Y, 2));
                if (offsetM > MAXIMUM_OFFSET_BEFORE_RESET)
                {
                    badOffsetCount++;
                }
                else
                {
                    badOffsetCount = 0;
                }
                if (badOffsetCount >= MAXIMUM_COUNT_BEFORE_RESET)
                {
                    //TODO Instead of reset, just shift background
                    //Log.Logger.Warning($"Background reset due to large offset ({offsetM})");
                    Initialise();
                    return;
                }
            }

            //Get greyscale of input image
            var greyscale = Process(mat);
            var rgb = mat.Clone();

            //Create mask of the contour regions
            var mask = new Mat(mat.Size, DepthType.Cv8U, 1);
            mask.SetTo(new MCvScalar(0));
            if (contours.Count > 0)
            {
                VectorOfVectorOfPoint contourArray = new VectorOfVectorOfPoint(contours.Select(x => x.Points).ToArray());
                CvInvoke.FillPoly(mask, contourArray, new MCvScalar(255));
            }

            //Add to stack if it has been initialised
            if (wasFirstImageReceived)
            {
                //Calculate offset if needed
                PointF offset;
                if (offsetNullable == null)
                {
                    offset = CalculateOffset(greyscale);
                }
                else
                {
                    offset = offsetNullable.Value;
                }
                //Invert as we will apply to the input image instead
                offset.X *= -1;
                offset.Y *= -1;

                //Translate
                greyscale.Translate(offset, BorderType.Reflect101);
                rgb.Translate(offset, BorderType.Reflect101);
                mask.Translate(offset, BorderType.Constant);

                //Replace any regions inside the contours with parts from the current model.          
                using (var oldPart = GreyscaleModel.Clone())
                {
                    oldPart.CopyTo(greyscale, mask);
                    //CvInvoke.BitwiseAnd(oldPart, mask, oldPart); //Zero outside of region (inside mask) in the model clone
                    //CvInvoke.BitwiseNot(mask, mask);                      //Invert mask
                    //CvInvoke.BitwiseAnd(greyscale, mask, greyscale);       //Zero inside of region in the current image
                    //CvInvoke.Add(greyscale, oldPart, greyscale);           //Add them together
                }

                //Replace any regions inside the contours with parts from the current model.        
                //CvInvoke.BitwiseNot(mask, mask);
                using (var oldPart = ColourModel.Clone())
                {
                    oldPart.CopyTo(rgb, mask);
                }

                //Insert images into stack
                CvInvoke.InsertChannel(greyscale, greyStack, currentIdx);
                rgbMats[currentIdx] = rgb.Clone();
                currentIdx++;
                if (currentIdx >= greyStack.NumberOfChannels) currentIdx = 0;
            }
            //Otherwise, initialise the stack
            else
            {
                //Get mean and standard deviation outside of mask area
                MCvScalar mean;
                MCvScalar std;
                using (var maskInv = mask.Clone())
                {
                    CvInvoke.BitwiseNot(maskInv, maskInv);
                    //TODO confirm that this works
                    mean = new MCvScalar();
                    std = new MCvScalar();
                    CvInvoke.MeanStdDev(greyscale, ref mean, ref std, maskInv);
                }

                //Set any really dark regions to the mean. This heaps avoid shadows from particles being in the initial model
                var lower = mean.V0 + 0.5 * std.V0;
                using (var maskLower = new Mat())
                {
                    CvInvoke.Threshold(greyscale, maskLower, lower, 255, ThresholdType.BinaryInv);
                    greyscale.SetTo(new MCvScalar(lower), maskLower);
                    rgb.SetTo(new MCvScalar(lower, lower, lower), maskLower);
                }

                //Now do the same thing for the masked regions
                greyscale.SetTo(new MCvScalar(lower), mask);
                rgb.SetTo(new MCvScalar(lower, lower, lower), mask);

                //Insert replicates into the stack
                for (int i = 0; i < greyStack.NumberOfChannels; i++)
                {
                    CvInvoke.InsertChannel(greyscale, greyStack, i);
                }
                for (int i = 0; i < rgbMats.Count; i++)
                {
                    rgbMats[i] = rgb.Clone();
                }
                wasFirstImageReceived = true;
            }
            mask.Dispose();
            greyscale.Dispose();
            rgb.Dispose();

            //Calculate the new model 
            RecalculateModel();
        }

        /// <summary>
        /// Processes image for insertion or comparison with the background stack
        /// </summary>
        /// <param name="mat">Input image</param>
        /// <returns>Processed copy of input image</returns>
        private Mat Process(Mat mat, DepthType depth = DepthType.Cv8U)
        {
            //Convert the image to greyscale if not already
            Mat temp;
            if (mat.NumberOfChannels != 1) temp = mat.CreateGreyscaleRedGreen();
            else temp = mat.Clone();

            //Ensure the correct bit depth
            if (temp.Depth != depth) temp.ConvertTo(temp, depth);
            return temp;
        }

        /// <summary>
        /// Calculate the background model image from the stack
        /// </summary>
        private void RecalculateModel()
        {
            //Copy the stack
            Mat stack = greyStack.Clone();

            //Sort it
            var channels = stack.NumberOfChannels;
            stack = stack.Reshape(1, width * height);
            CvInvoke.Sort(stack, stack, SortFlags.SortEveryRow & SortFlags.SortAscending);

            //Store as sorted stack
            sortedGreyStack?.Dispose();
            sortedGreyStack = stack.Reshape(channels, height);

            //Update the model
            var index = selectionIndex >= channels ? channels - 1 : selectionIndex;
            CvInvoke.ExtractChannel(sortedGreyStack, GreyscaleModel, selectionIndex);

            //Colour update
            var indexMat = greyStack.ArgsortAlong3rdDimension();
            CvInvoke.ExtractChannel(indexMat, indexMat, selectionIndex);
            indexMat.Reshape(1);
            indexMat.ConvertTo(indexMat, DepthType.Cv8U);
            ColourModel = rgbMats.SliceFromListIndex(indexMat);

            indexMat.Dispose();
            stack.Dispose();

            ModelUpdatedEvent(this);
        }

        public void OnSelectedIndexChanged()
        {
            RecalculateModel();
            //Grab new model image
            //var channels = sortedGreyStack.NumberOfChannels;
            //var index = selectionIndex >= channels ? channels - 1 : selectionIndex;
            //CvInvoke.ExtractChannel(sortedGreyStack, GreyscaleModel, index);
            //ModelUpdatedEvent(this);
        }

        public void OnStackSizeChanged()
        {
            rgbMats.Clear();
            for (int i = 0; i < stackSize; i++)
            {
                rgbMats.Add(ColourModel.Clone());
            }
        }
    }
}
