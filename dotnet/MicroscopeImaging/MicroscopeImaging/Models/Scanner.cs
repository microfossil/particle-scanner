using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using Emgu.CV;
using Emgu.CV.CvEnum;
using WPFHelpers.Helpers;

namespace MicroscopeImaging.Models
{
    public class Scanner : ObservableBase
    {
        private StageBase stage;
        private volatile bool stopScan = false;

        #region Properties

        // Lens magnification
        private double magnification = 4;

        public double Magnification
        {
            get => magnification;
            set
            {
                magnification = value;
                Calculate();
                NotifyPropertyChanged();
            }
        }

        // Lens magnification factor (for non telecentric where the distance changes the zoom)
        private double magnificationFactor = 1;

        public double MagnificationFactor
        {
            get => magnificationFactor;
            set
            {
                magnificationFactor = value;
                Calculate();
                NotifyPropertyChanged();
            }
        }

        // Bottom left point of the scanning area (stage units)
        private Point bottomLeft;

        public Point BottomLeft
        {
            get => bottomLeft;
            set => Set(ref bottomLeft, value);
        }

        // Top right point of the scanning area (stage units)
        private Point topRight;

        public Point TopRight
        {
            get => topRight;
            set => Set(ref topRight, value);
        }

        //Floor position of stack (units)
        private int stackFloor;
        public int StackFloor
        {
            get => stackFloor;
            set => Set(ref stackFloor, value);
        }

        // Height of stack (micrometers)
        private int stackHeight;
        public int StackHeight
        {
            get => stackHeight;
            set => Set(ref stackHeight, value);
        }

        // Stack step (micrometers)
        private int stackStep = 100;
        public int StackStep
        {
            get => stackStep;
            set
            {
                stackStep = value;
                Calculate();
                NotifyPropertyChanged();
            }
        }

        // Current index in stack
        private int stackIndex = 0;
        public int StackIndex
        {
            get => stackIndex;
            set => Set(ref stackIndex, value);
        }

        // Stack count
        private int stackCount = 0;
        public int StackCount
        {
            get => stackCount;
            set => Set(ref stackCount, value);
        }

        // Size of the image grid to process
        private Vector gridSize;
        public Vector GridSize
        {
            get => gridSize;
            private set => Set(ref gridSize, value);
        }

        // How much to step between them (micrometers)
        private Vector gridStep;
        public Vector GridStep
        {
            get => gridStep;
            private set => Set(ref gridStep, value);
        }

        // How much overlap (this will affect gridStep and gridSize
        private double gridOverlap = 0.25;
        public double GridOverlap
        {
            get => gridOverlap;
            set
            {
                gridOverlap = value;
                Calculate();
            }
        }

        private bool isScanning = false;
        public bool IsScanning
        {
            get => isScanning;
            set
            {
                isScanning = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        private BaslerCamera camera;

        public Scanner()
        {
            stage = ModelLocator.Instance.Stage;
            camera = ModelLocator.Instance.Camera;
            Calculate();
        }

        public void SetBottomLeft()
        {
            BottomLeft = new Point(stage.X, stage.Y);
            Calculate();
        }

        public void SetTopRight()
        {
            TopRight = new Point(stage.X, stage.Y);
            Calculate();
        }

        public void SetStackFloor()
        {
            StackFloor = stage.Z;
        }

        public void GoToBottomLeft()
        {
            stage.Move((int) BottomLeft.X, (int) BottomLeft.Y, StackFloor);
        }

        public void GoToTopLeft()
        {
            stage.Move((int)BottomLeft.X, (int)TopRight.Y, StackFloor);
        }

        public void GoToTopRight()
        {
            stage.Move((int) TopRight.X, (int) TopRight.Y, StackFloor);
        }

        public void GoToBottomRight()
        {
            stage.Move((int)TopRight.X, (int)BottomLeft.Y, StackFloor);
        }

        public void GoToStackFloor()
        {
            stage.MoveZ(StackFloor);
        }

        public void StopScan()
        {
            stopScan = true;
        }

        public void Calculate()
        {
            // Size of the image in units
            var imageXUnits = BaslerCamera.PixelWidth * BaslerCamera.Width / Magnification * MagnificationFactor /
                              stage.MicrometersPerUnit;
            var imageYUnits = BaslerCamera.PixelWidth * BaslerCamera.Height / Magnification * MagnificationFactor /
                              stage.MicrometersPerUnit;
            var scanningSize = topRight - bottomLeft;

            // Size of the grid
            var stepX = ((1 - GridOverlap) * imageXUnits);
            var stepY = ((1 - GridOverlap) * imageYUnits);
            var countX = Math.Ceiling((Math.Abs(scanningSize.X) - imageXUnits) / stepX);
            var countY = Math.Ceiling((Math.Abs(scanningSize.Y) - imageYUnits) / stepY);
            GridStep = new Vector(stepX, stepY);
            GridSize = new Vector(countX, countY);

            // Amount to step
            var count = (int) Math.Ceiling((double)StackHeight / StackStep);
            StackCount = count + 2;
        }

        public async Task StartScan()
        {
            bool nextLine = false;
            bool nextStack = false;
            stopScan = false;
            IsScanning = true;

            int dirX = (TopRight.X - BottomLeft.X) > 0 ? 1 : -1;
            int dirY = (TopRight.Y - BottomLeft.Y) > 0 ? 1 : -1;
            int dirZ = 1;

            var saver = new AquisitionDataflow();
            saver.Start();

            var baseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\scanner";

            //Move to state
            GoToBottomLeft();
            await Task.Delay(5000);
            var baseX = stage.VirtualX;
            var baseY = stage.VirtualY;
            var baseZ = stage.VirtualZ;

            for (int y = 0; y < GridSize.Y; y++)
            {
                for (int x = 0; x < GridSize.X; x++)
                {
                    for (int z = 0; z < StackCount; z++)
                    {
                        if (stopScan) goto Finish;
                        StackIndex = z;
                        string filename = $@"{baseDirectory}\\{stage.VirtualY-baseY:D6}\\{stage.VirtualY-baseY:D6}_{stage.VirtualX-baseX:D6}\\{stage.VirtualZ-baseZ:D6}.tiff";
                        Debug.WriteLine(filename);
                        Mat mat = camera.CloneCurrentImage();
                        CvInvoke.CvtColor(mat, mat, ColorConversion.Bgr2Gray);
                        await saver.SaveMat(mat, filename);
                        if (z < StackCount - 1)
                        {
                            stage.MoveRelative("z", true, (int)(dirZ * StackStep / stage.MicrometersPerUnit));
                            await Task.Delay(100);
                        }
                    }
                    dirZ *= -1;
                    if (x < GridSize.X - 1)
                    {
                        stage.MoveRelative("x", true, (int)(dirX * GridStep.X));
                        await Task.Delay(300);
                    }
                }
                dirX *= -1;
                if (y < GridSize.Y - 1)
                {
                    stage.MoveRelative("y", true, (int)(dirY * GridStep.Y));
                    await Task.Delay(300);
                }
            }
            Finish:
            IsScanning = false;
            saver.Stop();
        }
    }
}