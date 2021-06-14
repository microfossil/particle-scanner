using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Text.Core;
using Emgu.CV;
using ImageProcessing.Extensions;

namespace MicroscopeImaging.Models
{
    public class Calibrator
    {
        private BaslerCamera camera;
        private StageBase stage;

        public Calibrator(StageBase stage, BaslerCamera camera)
        {
            this.camera = camera;
            this.stage = stage;
        }

        public async void CalibrateResolution()
        {
            var count = 20;
            var xs = new float[count];
            var ys = new float[count];

            for (int i = 0; i < count; i++)
            {
                // Take image
                BaslerCamera.Pause = true;
                Mat firstImage = camera.CloneCurrentImage();
                BaslerCamera.Pause = false;
                stage.MoveRelative("x", true, stage.SmallStepXY*2);
                stage.MoveRelative("y", true, stage.SmallStepXY*2);
                await Task.Delay(1000);
                BaslerCamera.Pause = true;
                Mat secondImage = camera.CloneCurrentImage();
                BaslerCamera.Pause = false;
                firstImage = firstImage.CreateGreyscaleRedGreen();
                secondImage = secondImage.CreateGreyscaleRedGreen();
                var offset = firstImage.CalculatePhaseCorrection(secondImage);
                Debug.WriteLine(offset);
                Debug.WriteLine(Math.Sqrt(offset.X * offset.X + offset.Y * offset.Y) / Math.Sqrt(2));
                xs[i] = offset.X;
                ys[i] = offset.Y;
            }

            var xmean = xs.Average();
            var ymean = ys.Average();
            Debug.WriteLine(xmean);
            Debug.WriteLine(ymean);
            Debug.WriteLine(Math.Sqrt(xmean * xmean + ymean * ymean) / Math.Sqrt(2));
        }
    }
}
