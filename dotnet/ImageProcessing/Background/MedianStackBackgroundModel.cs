using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using GalaSoft.MvvmLight;
using ImageProcessing.Extensions;
using ImageProcessing.Segmentation;

namespace ImageProcessing.Background
{
    public class MedianStackBackgroundModel : ObservableObject, IBackgroundModel
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

        //Multiple separate stacks
        private List<Mat> stacks;
        private int[] currentIdx;
        private bool[] stackInitialisedFlag;
        private List<Mat> sortedStacks;
        public List<Mat> Models { get; private set; }

        private const int MAXIMUM_OFFSET_BEFORE_RESET = 150;
        private const int MAXIMUM_COUNT_BEFORE_RESET = 3;
        private int[] badOffsetCount;
        
        //Width and Height
        private int width = 0;
        private int height = 0;
        private int numModels = 0;

        public event Action<IBackgroundModel, int> ModelUpdatedEvent = delegate { };

        private int initialThreshold;
        public int InitialThreshold
        {
            get => initialThreshold;
            set => Set(ref initialThreshold, value);
        }
        
        private int stackSize;
        public int StackSize
        {
            get => stackSize;
            set
            {
                stackSize = value;
                if (selectionIndex >= stackSize) SelectionIndex = stackSize - 1;
                RaisePropertyChanged();
            }
        }

        private int selectionIndex;
        public int SelectionIndex
        {
            get => selectionIndex;
            set
            {
                selectionIndex = value;
                OnSelectedIndexChanged();
                RaisePropertyChanged();
            }
        }

        public Mat Model(int idx)
        {
            return Models[idx];
        }

        public MedianStackBackgroundModel(int width, int height, int numModels, int stackSize, int index, int initialThreshold)
        {
            if (index >= stackSize) throw new ArgumentOutOfRangeException(nameof(index), @"Index must be less than stack size");
            this.width = width;
            this.height = height;
            StackSize = stackSize;
            SelectionIndex = index;
            this.numModels = numModels;
            InitialThreshold = initialThreshold;

            Initialise();
        }

        /// <summary>
        /// Initialising the background stacks
        /// </summary>
        public void Initialise()
        {
            stacks = new List<Mat>(numModels);
            sortedStacks = new List<Mat>(numModels);
            Models = new List<Mat>(numModels);
            currentIdx = new int[numModels];
            stackInitialisedFlag = new bool[numModels];

            foreach (Mat mat in stacks)
            {
                mat.Dispose();
            }

            foreach (Mat mat in sortedStacks)
            {
                mat.Dispose();
            }

            foreach (Mat mat in Models)
            {
                mat.Dispose();
            }

            for (int i = 0; i < numModels; i++)
            {
                //Initialising the stack
                var mat = new Mat(height, width * stackSize, DepthType.Cv8U, 1);
                mat.SetTo(new MCvScalar(initialThreshold));
                mat = mat.Reshape(stackSize);
                stacks.Insert(i, mat);

                //Initialising the sortedStack
                mat = new Mat(height, width * stackSize, DepthType.Cv8U, 1);
                mat.SetTo(new MCvScalar(initialThreshold));
                mat = mat.Reshape(stackSize);
                sortedStacks.Insert(i, mat);

                //Initialising the models
                mat = new Mat(height, width, DepthType.Cv8U, 1);
                mat.SetTo(new MCvScalar(initialThreshold));
                Models.Insert(i, mat);
                ModelUpdatedEvent(this, i);

                //Other
                currentIdx[i] = 0;
                badOffsetCount[i] = 0;
                stackInitialisedFlag[i] = false;
            }
        }

        /// <summary>
        /// Calculate the difference between the input image and the background model
        /// </summary>
        /// <param name="mat">Input image</param>
        /// <param name="idx">Background model index</param>
        /// <returns>Difference image</returns>
        public (Mat, PointF) Difference(Mat mat, int idx)
        {
            Mat diff = Process(mat);

            PointF offset = new PointF(0, 0);

            //Has the background model been initialised?
            if (stackInitialisedFlag[idx] == false)
            {
                //If no, we compare against the mean of the pixel values outside of the particle
                //The boundaries of the particle are determined using the threshold "InitialThreshold"
                var mask = new Mat();
                CvInvoke.Threshold(diff, mask, initialThreshold, 255, ThresholdType.BinaryInv);
                MCvScalar mean = new MCvScalar();
                MCvScalar std = new MCvScalar();
                CvInvoke.MeanStdDev(diff, ref mean, ref std);
                diff.Subtract(mean.V0);
                mask.Dispose();
            }
            else
            {
                //If it has, we correct for any image shift (e.g. from the camera vibrating),
                //find the difference, and then translate back by the same amount.
                Mat brightRemoved = diff.Clone();
                CvInvoke.Threshold(brightRemoved, brightRemoved, InitialThreshold, InitialThreshold,
                    ThresholdType.Trunc);
                brightRemoved.ConvertTo(brightRemoved, DepthType.Cv32F);
                offset = brightRemoved.CalculatePhaseCorrection(Models[idx]);
                brightRemoved.Dispose();
                diff.Translate(offset, BorderType.Reflect);
                //offset = CorrectOffset(brightRemoved, idx);
                CvInvoke.Subtract(diff, Models[idx], diff);
                offset.X *= -1;
                offset.Y *= -1;
                diff.Translate(offset, BorderType.Constant, new MCvScalar(0));
            }
            return (diff, offset);
        }

        /// <summary>
        /// Calculates and the offset between the input image and the background model and corrects the input image in place.
        /// </summary>
        /// <param name="mat">Input image that has already been processed</param>
        /// <param name="idx">Background model index</param>
        /// <param name="maxOffset">Maximum allowable offset</param>
        /// <returns>Calculated offset</returns>
        public PointF CorrectOffset(Mat mat, int idx, double maxOffset = 1000)
        {
            //Create image to correlate
            //TODO put all of this in a single method - we already have Process... use that?
            //Mat greyscale = Process(mat, DepthType.Cv32F);
            //if (greyscale.NumberOfChannels > 1) greyscale = greyscale.CreateGreyscaleRedGreen();
            //else greyscale.ConvertTo(greyscale, DepthType.Cv32F);
            //Mat greyscale = mat.Clone();
            
            //Get model
            //Todo shouldnt we just get the current model??
            Mat model = new Mat();
            CvInvoke.ExtractChannel(sortedStacks[idx], model, selectionIndex);
            model.ConvertTo(model, DepthType.Cv32F);

            //Calculate offset using phase correlation
            PointF offset = mat.CalculatePhaseCorrection(model);
            //MCvPoint2D64f offset = CvInvoke.PhaseCorrelate(greyscale, model, null, out double response);
            model.Dispose();
            //greyscale.Dispose();
            
            //Translate image
            PointF offsetToApply = new PointF(0, 0);
            if (Math.Abs(offset.X) < maxOffset && Math.Abs(offset.Y) < maxOffset)
            {
                offsetToApply = new PointF(offset.X, offset.Y);
                mat.Translate(offsetToApply, BorderType.Reflect);
            }
            return offsetToApply;
        }

        /// <summary>
        /// Update the background model with the input image
        /// </summary>
        /// <param name="mat">Input image</param>
        /// <param name="contours">Contours of particles in image (regions to ignore)</param>
        /// <param name="idx">Background model index</param>
        public void Update(Mat mat, List<Contour> contours, int idx)
        {
            //Any preprocessing, e.g. greyscale conversion or filtering.
            //Image must be 8 bit greyscale
            var temp = Process(mat);

            //Create mask of the contour regions
            var mask = new Mat(mat.Size, DepthType.Cv8U, 1);
            mask.SetTo(new MCvScalar(0));
            if (contours.Count > 0)
            {
                VectorOfVectorOfPoint contourArray = new VectorOfVectorOfPoint(contours.Select(x => x.Points).ToArray());
                CvInvoke.FillPoly(mask, contourArray, new MCvScalar(255));
            }

            //Add to stack if it is initialised
            if (stackInitialisedFlag[idx])
            {
                //Correct any offset
                PointF offset = CorrectOffset(temp, idx);
                mask.Translate(offset, BorderType.Constant);

                //Replace any regions inside the contours with parts from the current model.          
                var oldPart = Models[idx].Clone();
                CvInvoke.BitwiseAnd(oldPart, mask, oldPart); //Zero outside of region (inside mask) in the model clone
                CvInvoke.BitwiseNot(mask, mask);             //Invert mask
                CvInvoke.BitwiseAnd(temp, mask, temp);       //Zero inside of region in the current image
                CvInvoke.Add(temp, oldPart, temp);           //Add them together
                oldPart.Dispose();                

                //Insert into stack
                CvInvoke.InsertChannel(temp, stacks[idx], currentIdx[idx]);
                currentIdx[idx]++;
                if (currentIdx[idx] >= stacks[idx].NumberOfChannels) currentIdx[idx] = 0;

                //Check if offsets are out of bounds
                var offsetM = Math.Sqrt(Math.Pow(offset.X, 2) + Math.Pow(offset.Y, 2));
                if (offsetM > MAXIMUM_OFFSET_BEFORE_RESET)
                {
                    badOffsetCount[idx]++;
                    //TODO
                    //Log.Logger.Warning("Large offset detected for background of receptacle {ReceptacleIndex}",idx);
                }
                else
                {
                    badOffsetCount[idx] = 0;
                }
                if (badOffsetCount[idx] > MAXIMUM_COUNT_BEFORE_RESET)
                {
                    
                    //badOffsetCount[idx];
                }
            }
            //Otherwise, we initialise the stack
            else
            {
                //Invert the mask
                var maskInv = mask.Clone();
                CvInvoke.BitwiseNot(maskInv, maskInv);
                //Get mean and standard deviation
                //TODO confirm that this works
                MCvScalar mean = new MCvScalar();
                MCvScalar std = new MCvScalar();
                CvInvoke.MeanStdDev(temp, ref mean, ref std, maskInv);
                maskInv.Dispose();

                var lower = mean.V0 + 0.5 * std.V0;
                //Set any really dark regions to the mean. This heaps avoid shadows from particles being in the initial model
                var maskLower = new Mat();
                CvInvoke.Threshold(temp, maskLower, lower, 255, ThresholdType.BinaryInv);
                temp.SetTo(new MCvScalar(lower), maskLower);
                maskLower.Dispose();

                //Now do the same thing 
                temp.SetTo(new MCvScalar(lower), mask);
                
                //Create new stack
                stacks[idx] = new Mat(height, width, DepthType.Cv8U, stackSize);
                //Insert everything into the stack
                for (int i = 0; i < stacks[idx].NumberOfChannels; i++)
                {
                    CvInvoke.InsertChannel(temp, stacks[idx], i);
                }
                stackInitialisedFlag[idx] = true;
            }
            mask.Dispose();
            temp.Dispose();

            //Calculate the new model 
            Calculate(idx);
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
            //Add original image to result of difference of Gaussian filter to enhance edges
            //Mat d0 = new Mat();
            //Mat d1 = new Mat();
            //CvInvoke.GaussianBlur(temp, d0, new Size(13, 13), 2);
            //CvInvoke.GaussianBlur(temp, d1, new Size(31, 31), 5);
            //CvInvoke.Subtract(d0, d1, d0);
            //d0.Multiply(3);
            //CvInvoke.Add(temp, d0, temp);
            //d0.Dispose();
            //d1.Dispose();
            if (temp.Depth != depth) temp.ConvertTo(temp, depth);
            return temp;
        }

        /// <summary>
        /// Calculate the background model image from the stack
        /// </summary>
        /// <param name="idx">Background model index</param>
        private void Calculate(int idx)
        {
            //Copy the stack
            Mat stack = stacks[idx].Clone();
            //Sort it
            var channels = stack.NumberOfChannels;                
            stack = stack.Reshape(1, width * height);
            CvInvoke.Sort(stack, stack, SortFlags.SortEveryRow & SortFlags.SortAscending);
            //Store as sorted stack
            sortedStacks[idx] = stack.Reshape(channels, height);
            stack.Dispose();
            //Update the model
            var index = selectionIndex >= channels ? channels - 1 : selectionIndex;
            CvInvoke.ExtractChannel(sortedStacks[idx], Models[idx], index);
            ModelUpdatedEvent(this, idx);
        }
        
        public void OnSelectedIndexChanged()
        {
            for (int i = 0; i < numModels; i++)
            {
                //Grab new model image
                var channels = sortedStacks[i].NumberOfChannels;
                var index = selectionIndex >= channels ? channels - 1 : selectionIndex;
                CvInvoke.ExtractChannel(sortedStacks[i], Models[i], index);
                ModelUpdatedEvent(this, i);
            }
        }
    }
}
