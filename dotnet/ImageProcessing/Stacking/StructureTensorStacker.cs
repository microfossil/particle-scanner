using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using ImageProcessing.Extensions;
using ImageProcessing.Filters;
using ImageProcessing.Images;

namespace ImageProcessing.Stacking
{
    public class StructureTensorStacker : IStacker
    {
        /*
         * Structure Tensor Stacker
         *
         * Fuses a multi-focal image stack into a single all-in-focus image.
         *
         * Images are `Add`ed to the Stacker, with a flag to indicate if it is the first image in the stack.
         * The images are placed in a queue and processed on another thread.
         * Processing takes an image, corrects for translation offsets with the previous image in the stack, e.g. due to vibration
         * then calculates the energy of the structure tensor for the image.
         *
         * When 'Stack' is called, the structure tensor energy is used to determine which image in the stack is most in focus at each point.
         * The index of the most in focus image is proportional to the depth from the camera, and so creates a depth map (pseudo-3D).
         * The depth map is smoothed and a final image is created by taking the pixel values from the two corresponding images.
         * E.g. if the smoothed depth at a location is 4.2, the pixel value with be 0.8 of image 4 and 0.2 of image 5.
         *
         * A final `StackedMat` is returned, containing the fused image plus the individual stacked images.
         */

        //Single focus images input into the Stacker
        private const int MAX_NUMBER_IN_STACK = 10;
        private BlockingCollection<Mat> inputCollection = new BlockingCollection<Mat>(MAX_NUMBER_IN_STACK);
        //The same images, but translation corrected using the phase correlation method
        private BlockingCollection<(Mat[],Mat,Mat[])> correctedCollection = new BlockingCollection<(Mat[],Mat,Mat[])>();
        //The image to use as the correction reference (the previous image in the stack)
        private volatile Mat redCorrectionReference = null;
        private volatile Mat greenCorrectionReference = null;
        private volatile Mat blueCorrectionReference = null;
        //This is set when we want to signal that correction begins again, e.g. when we are starting a new stack
        private volatile bool resetCorrectionFlag = true;
        //Signals that the image buffer is empty. Used to make sure translation correction has completed before stacking
        private ManualResetEvent bufferEmptyEvent = new ManualResetEvent(false);
        
        //Image dimensions
        private int width;
        private int height;
        private const bool USE_I1D = false;

        //Filters used for structure tensor calculation
        private Mat vKernel;
        private Mat hKernel;

        //Smoothing constants
        private const int ENERGY_SIGMA = 9;
        private const int DEPTH_SIGMA = 5;
        private const int DEPTH_TO_IMAGE_FACTOR = 20;
        private const int DEPTH_CHANNEL = 1;
        
        
        public StructureTensorStacker(int width, int height)
        {
            this.width = width;
            this.height = height;

            (vKernel, hKernel) = FilterKernel.LogGabor1stOrderWavelength8Sigma0p5();
        }

        public void Start()
        {
            StartAddProcessor();
        }

        public void Stop()
        {
            inputCollection.CompleteAdding();
        }             

        private void StartAddProcessor()
        {
            //DebugHelper.MethodInfo("Add Processor Started");
            
            Task.Run(() =>
            {
                while (!inputCollection.IsCompleted)
                {
                    try
                    {
                        Mat colour = inputCollection.Take();
                        bufferEmptyEvent.Reset();

                        Mat[] channels = colour.Split();
                        //Array.ForEach(channels, m => m.ConvertTo(m, DepthType.Cv32F));
                        //Mat greyscale = colour.CreateGreyscaleRedGreen();
                        Mat[] energy = { new Mat(), new Mat(), new Mat() };

                        //First image in batch
                        if (resetCorrectionFlag == true)
                        {
                            if (redCorrectionReference != null) redCorrectionReference.Dispose();
                            //if (greenCorrectionReference != null) greenCorrectionReference.Dispose();
                            //if (blueCorrectionReference != null) blueCorrectionReference.Dispose();
                            //correctionReference = greyscale.Clone();
                            redCorrectionReference = channels[2].Clone();
                            //greenCorrectionReference = channels[1];
                            //blueCorrectionReference = channels[0];
                            resetCorrectionFlag = false;
                            //DebugHelper.MethodInfo("Start phase calculation");
                            Task[] tasks =
                            {
                                Task.Run(() => { energy[0] = CalculateEnergy(channels[0]); }),
                                Task.Run(() => { energy[1] = CalculateEnergy(channels[1]); }),
                                Task.Run(() => { energy[2] = CalculateEnergy(channels[2]); })
                            };
                            Task.WaitAll(tasks);
                        }
                        else
                        {
                            //Greyscale image
                            //DebugHelper.MethodInfo("Start phase calculation");
                            var offset = channels[2].CalculatePhaseCorrection(redCorrectionReference);
                            //DebugHelper.MethodInfo("Start energy calculation");
                            Task[] tasks1 =
                            {
                                Task.Run(() => { energy[0] = CalculateEnergy(channels[0]); }),
                                Task.Run(() => { energy[1] = CalculateEnergy(channels[1]); }),
                                Task.Run(() => { energy[2] = CalculateEnergy(channels[2]); })
                            };
                            Task.WaitAll(tasks1);
                            //Task<PointF> phaseTask = Task.Run(() => { return greyscale.CalculatePhaseCorrection(correctionReference); });
                            //Task<Mat> energyTask = Task.Run(() => { return CalculateEnergy(greyscale); });
                            //Task.WaitAll(phaseTask, energyTask);
                            //var offset = phaseTask.Result;
                            //energy = energyTask.Result;                   

                            //DebugHelper.MethodInfo("Translate images");
                            Task[] tasks =
                            {
                                Task.Run(() => { channels[0].Translate(offset, BorderType.Reflect101); }),
                                Task.Run(() => { channels[1].Translate(offset, BorderType.Reflect101); }),
                                Task.Run(() => { channels[2].Translate(offset, BorderType.Reflect101); }),
                                Task.Run(() => { energy[0].Translate(offset, BorderType.Reflect101); }),
                                Task.Run(() => { energy[1].Translate(offset, BorderType.Reflect101); }),
                                Task.Run(() => { energy[2].Translate(offset, BorderType.Reflect101); }),
                                Task.Run(() => { colour.Translate(offset, BorderType.Reflect101); })
                            };
                            //Task t1 = Task.Run(() => { greyscale.Translate(offset, BorderType.Replicate); });
                            //Task t2 = Task.Run(() => { colour.Translate(offset, BorderType.Replicate); });
                            //Task t3 = Task.Run(() => { energy.Translate(offset, BorderType.Replicate); });
                            Task.WaitAll(tasks);
                            redCorrectionReference.Dispose();
                            redCorrectionReference = channels[2].Clone();
                        }
                        //DebugHelper.MethodInfo("Add");
                        correctedCollection.Add((channels, colour, energy));

                        //DebugHelper.MethodInfo("Add done");
                    }
                    catch (OperationCanceledException ex)
                    {
                        Debug.WriteLine("# STACKER #   Add processing cancelled\n" + ex.Message);
                    }
                    catch (InvalidOperationException ex)
                    {
                        Debug.WriteLine("# STACKER #   Add processing bailed\n" + ex.Message);
                    }
                    finally
                    {

                    }
                    if (inputCollection.Count == 0)
                    {
                        bufferEmptyEvent.Set();
                    }
                }
            });
        }

        /// <summary>
        /// Add image to the stack buffer
        /// </summary>
        /// <param name="mat">Input image</param>
        /// <param name="isNewStack">Start of new stack?</param>
        public void Add(Mat mat, bool isNewStack)
        {
            //If it is the first image in the stack, reset the translation correction
            if (isNewStack)
            {
                resetCorrectionFlag = true;
            }
            //Add the image
            var success = inputCollection.TryAdd(mat, 0);
            if (!success)
            {
                mat.Dispose();
            }
        }

        /// <summary>
        /// Calculate the local image energy using the structure tensor. This is used to determine which parts of the image are in focus.
        /// </summary>
        /// <param name="mat">Input image</param>
        /// <param name="blurSigma">Amount of Gaussian blur (sigma)</param>
        /// <returns>Structure tensor energy</returns>
        public Mat CalculateEnergy(Mat mat, int blurSigma = ENERGY_SIGMA)
        {
            var wid = blurSigma * 6 + 1;

            //Derivatives
            Mat x = mat.Clone();
            Mat y = mat.Clone();
            if (x.Depth == DepthType.Cv8U) x.ConvertTo(x, DepthType.Cv32F);
            if (y.Depth == DepthType.Cv8U) y.ConvertTo(y, DepthType.Cv32F);

            //Filter with log-gabor kernels
            //DebugHelper.MethodInfo("Log-Gabor Filter");
            Task xTask = Task.Run(() =>
            {
                CvInvoke.Filter2D(x, x, hKernel, new Point(-1, -1));
                CvInvoke.Multiply(x, x, x);
            });
            Task yTask = Task.Run(() =>
            {
                CvInvoke.Filter2D(y, y, vKernel, new Point(-1, -1));
                CvInvoke.Multiply(y, y, y);
            });
            Task.WaitAll(xTask, yTask);
            
            //Structure tensor
            //DebugHelper.MethodInfo("Structure Tensor");
            Mat energy = new Mat();
            CvInvoke.Add(x, y, energy);
            CvInvoke.Sqrt(energy, energy);
            //Blur
            CvInvoke.GaussianBlur(energy, energy, new Size(wid, wid), blurSigma);

            //Dispose
            x.Dispose();
            y.Dispose();
            
            return energy;
        }

        public StackedMat Stack()
        {
            //Wait for any processing to finish
            //DebugHelper.MethodInfo("Waiting for add processing to finish");
            bufferEmptyEvent.WaitOne();
            //DebugHelper.MethodInfo("Add processing finished");

            //Get the number of images in the stack
            var stackHeight = correctedCollection.Count;

            //Multi-channel Mat images to hold the greyscale, colour and depth images from the stack
            //Each channel is an image in the relevant stack
            //We can more easily manipulate the multi-channel Mat than a List of Mats
            //Mat greyscaleStack = new Mat(height, width, DepthType.Cv32F, stackHeight);
            //Mat redStack = new Mat(height, width, DepthType.Cv8U, stackHeight);
            //Mat greenStack = new Mat(height, width, DepthType.Cv8U, stackHeight);
            //Mat blueStack = new Mat(height, width, DepthType.Cv8U, stackHeight);
            //Mat energyMeasure = new Mat(height, width, DepthType.Cv32F, stackHeight);

            Mat[] channelStacks =
            {
                new Mat(height, width, DepthType.Cv8U, stackHeight),
                new Mat(height, width, DepthType.Cv8U, stackHeight),
                new Mat(height, width, DepthType.Cv8U, stackHeight)
            };

            Mat[] energyStacks =
            {
                new Mat(height, width, DepthType.Cv32F, stackHeight),
                new Mat(height, width, DepthType.Cv32F, stackHeight),
                new Mat(height, width, DepthType.Cv32F, stackHeight)
            };

            //Full colour stack as a list that will be returned
            List<Mat> stack = new List<Mat>();

            //Copy the images over (and dispose of them if necessary)
            for (int i = 0; i < stackHeight; i++)
            {
               // DebugHelper.MethodInfo($"Stack process{i}");
                (Mat[] channelsTemp, Mat colourTemp, Mat[] energyTemp) = correctedCollection.Take();

                for (int j = 0; j < 3; j++)
                {
                    CvInvoke.InsertChannel(channelsTemp[j], channelStacks[j], i);
                    CvInvoke.InsertChannel(energyTemp[j], energyStacks[j], i);
                    channelsTemp[j].Dispose();
                    energyTemp[j].Dispose();
                }
                stack.Add(colourTemp);
                ////Greyscale
                //CvInvoke.InsertChannel(gTemp, greyscaleStack, i);
                //gTemp.Dispose();
                ////Colour
                //Mat temp = new Mat();
                //CvInvoke.ExtractChannel(cTemp, temp, 0);
                //CvInvoke.InsertChannel(temp, redStack, i);
                //CvInvoke.ExtractChannel(cTemp, temp, 1);
                //CvInvoke.InsertChannel(temp, greenStack, i);
                //CvInvoke.ExtractChannel(cTemp, temp, 2);
                //CvInvoke.InsertChannel(temp, blueStack, i);
                //stack.Add(cTemp);
                //temp.Dispose();
                ////Energy
                //CvInvoke.InsertChannel(eTemp, energyMeasure, i);
                //eTemp.Dispose();
            }

            //Find maximum energy index
            //DebugHelper.MethodInfo("Energy");
            Mat[] depths = new Mat[3];
            Task[] tasks =
            {
                Task.Run(() => depths[0] = energyStacks[0].ArgsortAlong3rdDimension()),
                Task.Run(() => depths[1] = energyStacks[1].ArgsortAlong3rdDimension()),
                Task.Run(() => depths[2] = energyStacks[2].ArgsortAlong3rdDimension())
            };
            Task.WaitAll(tasks);
            CvInvoke.ExtractChannel(depths[0], depths[0], stackHeight - 1);
            CvInvoke.ExtractChannel(depths[1], depths[1], stackHeight - 1);
            CvInvoke.ExtractChannel(depths[2], depths[2], stackHeight - 1);
            //BUG in sortIdx, last channel is always max no matter what order
            //TODO Smooth to remove large transitions later

            //Convert green depth index to an image for output (depth index is multiplied by 20, so this works for up to 14 images in the stack)
            //Change DEPTH_TO_IMAGE_FACTOR to adjust this
            //DebugHelper.MethodInfo("Depth");
            Mat depthForOutput = new Mat();
            depths[DEPTH_CHANNEL].ConvertTo(depthForOutput, DepthType.Cv8U, DEPTH_TO_IMAGE_FACTOR);

            //Smooth the depth index in order to have soft transitions between image
            //DebugHelper.MethodInfo("Stack");
            Mat[] channels = new Mat[3];
            List<Task> taskList = new List<Task>();
            int[] inds = {0, 1, 2};
            foreach(var i in inds)
            {
                taskList.Add(Task.Run(() =>
                {
                    depths[i].ConvertTo(depths[i], DepthType.Cv32F);
                    CvInvoke.GaussianBlur(depths[i], depths[i], new Size(DEPTH_SIGMA * 6 + 1, DEPTH_SIGMA * 6 + 1), DEPTH_SIGMA);
                    //channels[i] = channelStacks[i].SliceFromChannelIndex(depths[i]);
                    channels[i] = channelStacks[i].InterpolateFromChannelIndex8U(depths[i]);
                }));
            }
            Task.WaitAll(taskList.ToArray());

            //DebugHelper.MethodInfo("Output");

            //Stacked greyscale
            //Mat greyscale = new Mat(new Size(width, height), DepthType.Cv32F, 1);
            //greyscale.SetTo(channels[2]);
            Mat greyscale = channels[DEPTH_CHANNEL].Clone();
            //channels[2].ConvertTo(greyscale, DepthType.Cv8U);

            //Stacked colour
            Mat colour = new Mat(new Size(width, height), DepthType.Cv8U, 3);
            CvInvoke.InsertChannel(channels[0], colour, 0);
            CvInvoke.InsertChannel(channels[1], colour, 1);
            CvInvoke.InsertChannel(channels[2], colour, 2);

            //Clear everything
            Array.ForEach(depths, m => m.Dispose());
            Array.ForEach(channels, m => m.Dispose());
            Reset();

            return new StackedMat(greyscale, colour, depthForOutput, stack);


            //DebugHelper.MethodInfo("Copy");

            ////Index as float array
            //float[] idxData = depth.CopyAsFloatArray();

            ////Greyscale stack to float array
            //float[] greyData = greyscaleStack.CopyAsFloatArray();

            ////RGB array to byte array
            //byte[] redData = redStack.CopyAsByteArray();
            //byte[] greenData = greenStack.CopyAsByteArray();
            //byte[] blueData = blueStack.CopyAsByteArray();

            ////Greyscale stacked output
            //float[] greyscaleStacked = new float[height * width];
            ////Color stacked output
            //byte[] colourStacked = new byte[height * width * 3];

            ////Make it
            //DebugHelper.MethodInfo("Make stacks");
            //for (int i = 0; i < height; i++)
            //{
            //    for (int j = 0; j < width; j++)
            //    {
            //        var d = idxData[i * width + j];

            //        int floor = (int)Math.Floor(d);
            //        int ceil = (int)Math.Ceiling(d);
            //        if (ceil >= stackHeight) ceil = stackHeight - 1;
            //        if (floor < 0) floor = 0;
            //        var frac = (float)(d - Math.Truncate(d));

            //        int offset = i * width * stackHeight + j * stackHeight;
            //        int rgbOffset = i * width * 3 + j * 3;
            //        greyscaleStacked[i * width + j] = greyData[offset + floor] * (1 - frac) + greyData[offset + ceil] * frac;
            //        colourStacked[rgbOffset] = (byte)(redData[offset + floor] * (1 - frac) + redData[offset + ceil] * frac);
            //        colourStacked[rgbOffset + 1] = (byte)(greenData[offset + floor] * (1 - frac) + greenData[offset + ceil] * frac);
            //        colourStacked[rgbOffset + 2] = (byte)(blueData[offset + floor] * (1 - frac) + blueData[offset + ceil] * frac);
            //    }
            //}


        }
        
        public void Reset()
        {
            //foreach (Mat mat in greyscaleList) mat.Dispose();
            //foreach (Mat mat in energyMeasureList) mat.Dispose();
            //foreach (Mat mat in colourList) mat.Dispose(); //Mats in colour list are returned with StackedMat
            //greyscaleList.Clear();
            //colourList.Clear();
            //energyMeasureList.Clear();
        }
    }
}
