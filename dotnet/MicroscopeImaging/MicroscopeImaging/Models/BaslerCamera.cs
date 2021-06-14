using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Basler.Pylon;
using Emgu.CV;
using GalaSoft.MvvmLight;
using WPFHelpers.Helpers;

namespace MicroscopeImaging.Models
{
    public class BaslerCamera : ObservableObject
    {
        public List<ICameraInfo> cameraList;
        Basler.Pylon.Camera camera = null;
        PixelDataConverter converter; // = new PixelDataConverter();

        public const int Width = 2448;
        public const int Height = 2048;

        public const double PixelWidth = 3.45;


        private Mat currentImage;

        // New image event
        public event EventHandler ImageEvent = delegate { };

        //Global bool to pause camera
        public static volatile bool Pause = false;

        public BaslerCamera()
        {
            converter = new PixelDataConverter();
        }

        #region Properties

        private Boolean isPresent = false;

        public Boolean IsPresent
        {
            get => isPresent;
            set => Set(ref isPresent, value);
        }

        private Boolean isConnected = false;

        public Boolean IsConnected
        {
            get => isConnected;
            set => Set(ref isConnected, value);
        }

        private double exposure;

        public double Exposure
        {
            get => exposure;
            set
            {
                if (isConnected) SetExposure(value);
                Set(ref exposure, value);
            }
        }

        private double gain;

        public double Gain
        {
            get => gain;
            set
            {
                if (isConnected) SetGain(value);
                Set(ref gain, value);
            }
        }

        private double redBalance;

        public double RedBalance
        {
            get => redBalance;
            set
            {
                if (isConnected) SetRedLevel(value);
                Set(ref redBalance, value);
            }
        }

        private double greenBalance;

        public double GreenBalance
        {
            get => greenBalance;
            set
            {
                if (isConnected) SetGreenLevel(value);
                Set(ref greenBalance, value);
            }
        }

        private double blueBalance;

        public double BlueBalance
        {
            get => blueBalance;
            set
            {
                if (isConnected) SetBlueLevel(value);
                Set(ref blueBalance, value);
            }
        }

        #endregion

        #region Camera Setters

        private void SetCameraLight()
        {
            if (!IsConnected) return;
            try
            {
                camera.Parameters[PLCamera.LightSourcePreset].TrySetValue(PLCamera.LightSourcePreset.Daylight5000K);
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine($"! ERROR !   Tried to set light preset");
            }
        }

        public void SetRedLevel(double value)
        {
            if (!IsConnected) return;
            try
            {
                camera.Parameters[PLCamera.BalanceRatioSelector].TrySetValue(PLCamera.BalanceRatioSelector.Red);
                camera.Parameters[PLCamera.BalanceRatio].TrySetValue(value);
                Console.WriteLine("# CAMERA #   Green: " + camera.Parameters[PLCamera.BalanceRatio].GetValue());
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine($"! ERROR !   Tried to set blue to {value}, current value " +
                                  camera.Parameters[PLCamera.BalanceRatio].GetValue());
            }
        }

        public void SetGreenLevel(double value)
        {
            if (!IsConnected) return;
            try
            {
                camera.Parameters[PLCamera.BalanceRatioSelector].TrySetValue(PLCamera.BalanceRatioSelector.Green);
                camera.Parameters[PLCamera.BalanceRatio].TrySetValue(value);
                Console.WriteLine("# CAMERA #   Green: " + camera.Parameters[PLCamera.BalanceRatio].GetValue());
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine($"! ERROR !   Tried to set blue to {value}, current value " +
                                  camera.Parameters[PLCamera.BalanceRatio].GetValue());
            }
        }

        public void SetBlueLevel(double value)
        {
            if (!IsConnected) return;
            try
            {
                camera.Parameters[PLCamera.BalanceRatioSelector].TrySetValue(PLCamera.BalanceRatioSelector.Blue);
                camera.Parameters[PLCamera.BalanceRatio].TrySetValue(value);
                Console.WriteLine("# CAMERA #   Green: " + camera.Parameters[PLCamera.BalanceRatio].GetValue());
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine($"! ERROR !   Tried to set blue to {value}, current value " +
                                  camera.Parameters[PLCamera.BalanceRatio].GetValue());
            }
        }

        private void SetExposure(double value)
        {
            if (!IsConnected) return;
            try
            {
                camera.Parameters[PLCamera.ExposureTime].TrySetValue(value);
                Console.WriteLine("# CAMERA #   Exposure: " + camera.Parameters[PLCamera.ExposureTime].GetValue());
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine($"! ERROR !   Tried to set exposure to {value}, current value "
                                  + camera.Parameters[PLCamera.ExposureTime].GetValue());
            }
        }

        private void SetGain(double value)
        {
            if (!IsConnected) return;
            camera.Parameters[PLCamera.Gain].TrySetValue(value);
            Console.WriteLine("# CAMERA #   Gain: " + camera.Parameters[PLCamera.Gain].GetValue());
        }

        public void SetFlipXY(bool value)
        {
            if (!IsConnected) return;
            camera.Parameters[PLCamera.ReverseX].SetValue(value);
            camera.Parameters[PLCamera.ReverseY].SetValue(value);
        }

        #endregion

        public void ToggleCamera()
        {
            if (!IsConnected)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        public void Start()
        {
            if (!isPresent) return;

            camera = new Camera(cameraList[0]);
            //CameraImage = new WriteableBitmap(2448, 2048, 0, 0, PixelFormats.Bgr24, null);
            //imageForSaving = new WriteableBitmap(2448, 2048, 0, 0, PixelFormats.Bgr24, null);

            // Starts the continuous grabbing of images and handles exceptions.            
            try
            {
                converter = new PixelDataConverter();
                converter.OutputPixelFormat = PixelType.BGR8packed;
                camera.CameraOpened += Basler.Pylon.Configuration.AcquireContinuous;
                camera.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                camera.Open();
                //Console.WriteLine("# CAMERA #   Gain auto: " + camera.Parameters[PLCamera.GainAuto].GetValue());
                //Console.WriteLine("# CAMERA #   Exposure: " + camera.Parameters[PLCamera.ExposureTime].GetValue());
                //Console.WriteLine("# CAMERA #   Gain: " + camera.Parameters[PLCamera.Gain].GetValue());
                camera.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                camera.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);

                IsConnected = true;

                exposure = 5000;
                gain = 0;
                redBalance = 1.8;
                greenBalance = 1.0;
                blueBalance = 1.4;

                //if (loadDefaults)
                //{
                //    Exposure = 3000;
                //    Gain = 0;
                //    RedBalance = 1.8;
                //    GreenBalance = 1.0;
                //    BlueBalance = 1.4;
                //}
                //else
                //{
                SetExposure(exposure);
                SetGain(gain);
                SetRedLevel(redBalance);
                SetGreenLevel(greenBalance);
                SetBlueLevel(blueBalance);
                //}
                Console.WriteLine("# CAMERA #   isConnected");
            }
            catch (Exception ex)
            {
                Console.WriteLine("! EXCEPTION !   " + ex.Message);
            }
        }

        public void Stop()
        {
            if (camera != null)
            {
                camera.Close();
                IsConnected = false;
                Console.WriteLine("# CAMERA #   Disconnected");
            }
        }

        // Occurs when an image has been acquired and is ready to be processed.
        private void OnImageGrabbed(Object sender, ImageGrabbedEventArgs e)
        {
            Mat mat = null;
            try
            {
                // Get the grab result.
                IGrabResult grabResult = e.GrabResult;

                // Check if the image can be displayed.
                if (grabResult.IsValid)
                {
                    //Convert to Mat format
                    mat = new Mat(2048, 2448, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
                    converter.Convert(mat.DataPointer, 2448 * 2048 * 3, grabResult);
                    UpdateCurrentImage(mat);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("! EXCEPTION !   " + ex.Message);
            }
            finally
            {
                // Dispose the grab result if needed for returning it to the grab loop.
                e.DisposeGrabResultIfClone();

                //Run event
                if (currentImage != null && Application.Current != null)
                {
                    try
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => { ImageEvent(this, null); }));
                    }
                    catch (NullReferenceException ex)
                    {

                    }
                }
            }
        }

        private void UpdateCurrentImage(Mat image)
        {
            lock (this)
            {
                currentImage?.Dispose();
                currentImage = image;
            }
        }

        public Mat CloneCurrentImage()
        {
            Mat clone;
            lock (this)
            {
                clone = currentImage.Clone();
            }
            return clone;
        }

        public void UpdateCameraList()
        {
            try
            {
                // Ask the camera finder for a list of camera devices.
                cameraList = CameraFinder.Enumerate();
                //Console.WriteLine(cameraList);
                IsPresent = cameraList.Count > 0;
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                Console.WriteLine("! EXCEPTION !   " + ex.Message);
            }
        }
    }
}