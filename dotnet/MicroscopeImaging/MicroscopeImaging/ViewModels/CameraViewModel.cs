using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using GalaSoft.MvvmLight;
using ImageProcessing.Extensions;
using MicroscopeImaging.Models;
using WPFHelpers.Helpers;

namespace MicroscopeImaging.ViewModels
{
    public class CameraViewModel : ViewModelBase
    {
        public enum ChannelType
        {
            Normal = -1,
            Red = 2,
            Green = 1,
            Blue = 0
        }

        private ChannelType displayChannel;
        public ChannelType DisplayChannel
        {
            get => displayChannel;
            set => Set(ref displayChannel, value);
        }

        private WriteableBitmap image;
        public WriteableBitmap Image
        {
            get => image;
            set => Set(ref image, value);
        }

        public BaslerCamera Camera { get; set; }

        private SemaphoreSlim cameraImageGuiUpdateSemaphoreSlim = new SemaphoreSlim(1);

        public CameraViewModel()
        { 
            Image = new WriteableBitmap(BaslerCamera.Width, BaslerCamera.Height, 0, 0, PixelFormats.Bgr24, null);
            Camera = ModelLocator.Instance.Camera;
            Camera.UpdateCameraList();
            DisplayChannel = ChannelType.Normal;
            Camera.ImageEvent += ImageEvent;
            Camera.Start();
            Camera.SetFlipXY(true);
        }

        private async void ImageEvent(object sender, EventArgs e)
        {
            //Update the display
            if (!BaslerCamera.Pause)
            {
                if (cameraImageGuiUpdateSemaphoreSlim.Wait(0))
                {
                    Mat mat = Camera.CloneCurrentImage();
                    if (DisplayChannel != ChannelType.Normal)
                    {
                        await Task.Run(() =>
                        {
                            try
                            {
                                int idx = (int)DisplayChannel;
                                Mat[] channels = mat.Split();
                                CvInvoke.InsertChannel(channels[idx], mat, 0);
                                CvInvoke.InsertChannel(channels[idx], mat, 1);
                                CvInvoke.InsertChannel(channels[idx], mat, 2);
                                Array.ForEach(channels, m => m.Dispose());
                            }
                            catch (Exception ex)
                            {
                                DebugHelper.MethodInfo(ex.Message);
                            }
                        });
                    }
                    mat.CopyToWriteableBitmap(image);
                    mat.Dispose();
                    cameraImageGuiUpdateSemaphoreSlim.Release();
                }
            }
        }
    }
}
