using System;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Emgu.CV;
using GalaSoft.MvvmLight.CommandWpf;
using ImageProcessing.Extensions;
using ImageProcessing.Images;
using ImageProcessing.Segmentation;
using ImageProcessing.Stacking;
using MicroscopeImaging.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using WPFHelpers.Helpers;
using BaslerCamera = MicroscopeImaging.Models.BaslerCamera;

namespace MicroscopeImaging
{
    public class MainViewModel : ObservableBase
    {
        //
        // #region Properties
        //
        // private double processingIntensityThreshold;
        //
        // public double ProcessingIntensityThreshold
        // {
        //     get { return processingIntensityThreshold; }
        //     set
        //     {
        //         processingIntensityThreshold = value;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // private double processingDiameterThreshold;
        // public double ProcessingDiameterThreshold
        // {
        //     get { return processingDiameterThreshold; }
        //     set
        //     {
        //         processingDiameterThreshold = value;
        //         ProcessingAreaThreshold = Math.Pow(value / 2, 2) * Math.PI * BaslerCamera.PixelAreaMM * 1000 * 1000;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // private double processingAreaThreshold;
        // public double ProcessingAreaThreshold
        // {
        //     get { return processingAreaThreshold; }
        //     private set
        //     {
        //         processingAreaThreshold = value;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // private double processingConvexityThreshold;
        // public double ProcessingConvexityThreshold
        // {
        //     get { return processingConvexityThreshold; }
        //     set
        //     {
        //         processingConvexityThreshold = value;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // private WriteableBitmap cameraImage;
        //
        // public WriteableBitmap CameraImage
        // {
        //     get { return cameraImage; }
        //     set
        //     {
        //         cameraImage = value;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // private double maxParticleWidth = 0.5;
        //
        // public double MaxParticleWidth
        // {
        //     get { return maxParticleWidth; }
        //     set
        //     {
        //         maxParticleWidth = value;
        //         int buffer = (int)(maxParticleWidth * 1000 / 2);
        //         Scanner.StepBuffer = buffer;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // private double zStackStep = 0.05;
        //
        // public double ZStackStep
        // {
        //     get { return zStackStep; }
        //     set
        //     {
        //         zStackStep = value;
        //         int step = (int)(zStackStep * 1000);
        //         Scanner.StepZ = step;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // private Int32 set;
        // public Int32 Set
        // {
        //     get { return set; }
        //     set
        //     {
        //         set = value;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // private Int32 subset;
        //
        // public Int32 Subset
        // {
        //     get { return subset; }
        //     set
        //     {
        //         subset = value;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // private String folder;
        //
        // public String Folder
        // {
        //     get { return folder; }
        //     set
        //     {
        //         folder = value;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // private String stem;
        //
        // public String Stem
        // {
        //     get { return stem; }
        //     set
        //     {
        //         stem = value;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // private COMPort selectedPort;
        //
        // public COMPort SelectedPort
        // {
        //     get { return selectedPort; }
        //     set
        //     {
        //         selectedPort = value;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // public class COMPort
        // {
        //     public String Name { get; set; }
        //
        //     public COMPort(String name)
        //     {
        //         this.Name = name;
        //     }
        // }
        //
        // private bool processingShow;
        //
        // public bool ProcessingShow
        // {
        //     get { return processingShow; }
        //     set
        //     {
        //         processingShow = value;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // private bool isDarkOnLight;
        // public bool IsDarkOnLight
        // {
        //     get { return isDarkOnLight; }
        //     set
        //     {
        //         isDarkOnLight = value;
        //         NotifyPropertyChanged();
        //     }
        // }
        //
        // #endregion
        //
        // //Basler Camera
        // public BaslerCamera Camera { get; set; } = new BaslerCamera();
        // public WriteableBitmap CurrentImage { get; set; }
        // public Mat CurrentMat { get; set; }
        //
        // //Stage
        // public StageBase Stage { get; set; }
        // public BindingList<COMPort> Ports { get; set; }
        //
        // //Scanner and image saver
        // public Scanner Scanner { get; set; }
        // ImageSaver imageSaver = new ImageSaver(100);
        //
        // //The window this is associated with
        // MainWindow parent;
        //
        // private const int LINE_REMOVAL_WIDTH = 31;
        //
        // //Stacker
        // private StructureTensorStacker stacker;
        //
        // //FPS timer
        // System.Timers.Timer fpsTimer = new System.Timers.Timer(1000);
        // int frames = 0;
        //
        // //Cross thread command if we need to know if a particle is present
        // private CrossThreadCommand<object, bool> IsParticlePresentCommunication = new CrossThreadCommand<object, bool>();
        //
        //
        // public MainViewModel(MainWindow control)
        // {
        //     //This window
        //     this.parent = control;
        //
        //     //EmailAlert alert = new EmailAlert("ross.g.marchant@gmail.com");
        //     //alert.Alarm("test");
        //     IsDarkOnLight = false;
        //
        //     //Index of the current images
        //     Set = 1;
        //     Subset = 1;
        //     
        //     //Stage
        //     Stage = new EnderStage();
        //     Ports = new BindingList<COMPort>();
        //
        //     //Scanner - pass in the routines to call when saving
        //     Scanner = new Scanner(Stage, IsStillSaving, IsParticlePresent);
        //     Scanner.ReadyForAcquisitionEvent += Scanner_ReadyForAcquisitionEvent;
        //     Scanner.ReadyForStackingEvent += Scanner_ReadyForStackingEvent;
        //
        //     //Default image
        //     CameraImage = new WriteableBitmap(BaslerCamera.Width, BaslerCamera.Height, 0, 0, PixelFormats.Bgr24, null);
        //     CurrentImage = new WriteableBitmap(BaslerCamera.Width, BaslerCamera.Height, 0, 0, PixelFormats.Bgr24, null);
        //
        //     //Camera
        //     Camera.CameraImageEvent += Camera_CameraImageEvent;
        //     Camera.UpdateCameraList();
        //     //Camera.StartCamera();
        //
        //     //Stacker
        //     stacker = new StructureTensorStacker(BaslerCamera.Width, BaslerCamera.Height);
        //     stacker.Start();
        //
        //     //FPS timer event
        //     fpsTimer.Elapsed += FpsTimer_Elapsed;
        //     fpsTimer.Start();
        //     imageSaver.Start();
        //
        //     //Closing event (save settings)
        //     control.Closing += Control_Closing;
        //
        //     InitSettings();
        //
        //     Camera.UpdateCameraList();
        //
        //     UpdatePortsList();
        //     if (Ports.Count > 0)
        //     {
        //         SelectedPort = Ports[Ports.Count - 1];
        //     }
        // }
        //
        // public bool IsParticlePresent()
        // {
        //     bool result = false;
        //     using (Mat diff = CurrentMat.CreateGreyscaleRedGreen8U())
        //     {
        //         if (IsDarkOnLight)
        //         {
        //             CvInvoke.BitwiseNot(diff, diff);
        //         }
        //         result = Segmenter.CheckForParticle(
        //             CurrentMat, 
        //             diff,
        //             (int)ProcessingIntensityThreshold,
        //             (int)ProcessingAreaThreshold,
        //             ProcessingConvexityThreshold,
        //             BaslerCamera.Width * Scanner.StepBuffer / Scanner.ImageWidth);
        //     }
        //     return result;
        // }
        //
        // private async void Scanner_ReadyForStackingEvent(object sender, int e)
        // {
        //     Console.WriteLine("STACKING");
        //
        //     BaslerCamera.Pause = true;
        //
        //     //StackedMat stackedMat = await Task.Run(() => { return stacker.Stack(); });
        //     StackedMat stackedMat = stacker.Stack();
        //
        //
        //     //string filename = String.Format("{0}\\{1}\\0000debug_{1}_{2:D4}.jpg", folder, stem, set);
        //     //SaveableBitmap saveImage = new SaveableBitmap(stackedMat.Colour, filename);
        //     //imageSaver.Add(saveImage);
        //
        //     BaslerCamera.Pause = false;
        //
        //     Mat diff = stackedMat.Greyscale.Clone();
        //     if (IsDarkOnLight)
        //     {
        //         CvInvoke.BitwiseNot(diff, diff);
        //     }
        //     var (crops, stackedCrops, contours) = Segmenter.Segment(stackedMat, 
        //         diff, 
        //         (int)ProcessingIntensityThreshold,
        //         (int)ProcessingAreaThreshold,
        //         ProcessingConvexityThreshold, 
        //         1.1,
        //         BaslerCamera.Width * Scanner.StepBuffer / Scanner.ImageWidth,
        //         LINE_REMOVAL_WIDTH);
        //     stackedMat.Release();
        //     diff.Dispose();
        //     GC.Collect();
        //     foreach (var crop in crops)
        //     {
        //         Console.WriteLine($"SET: {set} {Set}");
        //         string filename = String.Format("{0}\\{1}\\{1}_{2:D4}.jpg", folder, stem, set);
        //         SaveableBitmap saveImage = new SaveableBitmap(crop.Mat, filename);
        //         imageSaver.Add(saveImage);
        //         crop.Release();
        //         Set++;
        //     }
        //
        //     foreach (var crop in stackedCrops)
        //     {
        //         crop.Release();
        //     }
        //     GC.Collect();
        // }
        //
        // private void Scanner_ReadyForAcquisitionEvent(object sender, int e)
        // {
        //     Console.WriteLine("TAKING");
        //     BaslerCamera.Pause = true;
        //     stacker.Add(CurrentMat.Clone(), e == 0);
        //     GC.Collect();
        //     BaslerCamera.Pause = false;
        // }
        //
        // private void Camera_CameraImageEvent(object sender, CameraImageEventArgs e)
        // {
        //     Mat mat = e.Mat;
        //     if (!BaslerCamera.Pause)
        //     {
        //         ////Clone to send to acquisition system
        //         Mat sMat = mat.Clone();
        //
        //         //Show live segmentation?
        //         if (processingShow)
        //         {
        //             Mat diff = mat.CreateGreyscaleRedGreen8U();
        //             Segmenter.DrawSegments(
        //                 mat,
        //                 diff,
        //                 (int)ProcessingIntensityThreshold,
        //                 (int)ProcessingAreaThreshold,
        //                 ProcessingConvexityThreshold,
        //                 BaslerCamera.Width * Scanner.StepBuffer / Scanner.ImageWidth,
        //                 processingShow,
        //                 LINE_REMOVAL_WIDTH);
        //             diff.Dispose();
        //         }
        //         parent.Dispatcher.BeginInvoke(new Action(() =>
        //         {
        //             SetImage(sMat);
        //             mat.CopyToWriteableBitmap(cameraImage);
        //             mat.Dispose();
        //         }), DispatcherPriority.Render);
        //         frames++;
        //     }
        //     else
        //     {
        //         mat.Dispose();
        //     }
        // }
        //
        // public void SetImage(Mat mat)
        // {
        //     if (CurrentMat != null) CurrentMat.Dispose();
        //     CurrentMat = mat;
        //     mat.CopyToWriteableBitmap(CurrentImage);
        // }
        //
        // private void FpsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        // {
        //     Console.WriteLine(frames);
        //     frames = 0;
        // }
        //
        // private void Control_Closing(object sender, CancelEventArgs e)
        // {
        //     Camera.StopCamera();
        //     Stage.Disconnect();
        //     stacker.Stop();
        //     imageSaver.Stop();
        //     SaveSettings();
        // }
        //
        // /// <summary>
        // /// Checks if there are images still saving
        // /// </summary>
        // /// <returns>True if images are still saving</returns>
        // public bool IsStillSaving()
        // {
        //     return !imageSaver.IsEmpty();
        // }
        //
        // /// <summary>
        // /// Initialising the settings
        // /// </summary>
        // private void InitSettings()
        // {
        //     var settings = Properties.Settings.Default;
        //     Camera.CameraExposure = settings.Exposure;
        //     Camera.CameraGain = settings.Gain;
        //     ProcessingIntensityThreshold = settings.IntensityThreshold;
        //     ProcessingDiameterThreshold = settings.AreaThreshold;
        //     MaxParticleWidth = settings.MaxParticleWidth;
        //     ZStackStep = settings.ZStackStep;
        //     Scanner.StartX = settings.ScanAreaUpperLeftX;
        //     Scanner.StartY = settings.ScanAreaUpperLeftY;
        //     Scanner.EndX = settings.ScanAreaLowerRightX;
        //     Scanner.EndY = settings.ScanAreaLowerRightY;
        //     Scanner.StartZ = settings.ScanAreaCeilingZ;
        //     Scanner.EndZ = settings.ScanAreaFloorZ;
        //     Stem = settings.Stem;
        //     Scanner.Magnification = settings.Magnification;
        //     Scanner.MagnificationFactor = settings.MagnificationFactor;
        //
        //     if (Directory.Exists(settings.Folder))
        //     {
        //         Folder = settings.Folder;
        //     }
        //     else
        //     {
        //         Folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        //     }
        // }
        //
        // /// <summary>
        // /// Save the program settings
        // /// </summary>
        // private void SaveSettings()
        // {
        //     var settings = Properties.Settings.Default;
        //     settings.Exposure = Camera.CameraExposure;
        //     settings.Gain = Camera.CameraGain;
        //     settings.IntensityThreshold = ProcessingIntensityThreshold;
        //     settings.AreaThreshold = ProcessingDiameterThreshold;
        //     settings.MaxParticleWidth = MaxParticleWidth;
        //     settings.ZStackStep = ZStackStep;
        //     settings.ScanAreaUpperLeftX = Scanner.StartX;
        //     settings.ScanAreaUpperLeftY = Scanner.StartY;
        //     settings.ScanAreaLowerRightX = Scanner.EndX;
        //     settings.ScanAreaLowerRightY = Scanner.EndY;
        //     settings.ScanAreaCeilingZ = Scanner.StartZ;
        //     settings.ScanAreaFloorZ = Scanner.EndZ;
        //     settings.Stem = Stem;
        //     settings.Folder = Folder;
        //     settings.Magnification = Scanner.Magnification;
        //     settings.MagnificationFactor = Scanner.MagnificationFactor;
        //     settings.Save();
        // }
        //
        // public void ChangeFolder()
        // {
        //     BaslerCamera.Pause = true;
        //     CommonOpenFileDialog dialog = new CommonOpenFileDialog();
        //     dialog.InitialDirectory = Folder;
        //     dialog.IsFolderPicker = true;
        //     if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
        //     {
        //         Folder = dialog.FileName;
        //     }
        //     BaslerCamera.Pause = false;
        // }
        //
        // public void UpdatePortsList()
        // {
        //     //Stage.RefreshPorts();
        //     Ports.Clear();
        //     foreach (String s in SerialPort.GetPortNames())
        //     {
        //         Ports.Add(new COMPort(s));
        //         Console.WriteLine(s);
        //     }
        //     NotifyPropertyChanged("Ports");
        // }
        //
        // private void ToggleStageConnection(String name)
        // {
        //     if (Stage.IsConnected) Stage.Disconnect();
        //     else Stage.Connect(name);
        // }
        //
        // public void Reset()
        // {
        //     //Set = 1;
        //     Set++;
        //     Subset = 1;
        // }
        //
        // #region Commands
        //
        // private ICommand resetCommand;
        // public ICommand ResetCommand
        // {
        //     get
        //     {
        //         if (refreshCameraCommand == null)
        //         {
        //             refreshCameraCommand = new RelayCommand(
        //                 () => Reset(),
        //                 () => { return true; });
        //         }
        //         return resetCommand;
        //     }
        // }
        //
        // private ICommand refreshCameraCommand;
        // public ICommand RefreshCameraCommand
        // {
        //     get
        //     {
        //         if (refreshCameraCommand == null)
        //         {
        //             refreshCameraCommand = new RelayCommand(
        //                 () => Camera.UpdateCameraList(),
        //                 () => { return true; });
        //         }
        //         return startCameraCommand;
        //     }
        // }
        //
        // private ICommand startCameraCommand;
        // public ICommand StartCameraCommand
        // {
        //     get
        //     {
        //         if (startCameraCommand == null)
        //         {
        //             startCameraCommand = new RelayCommand(
        //                 () => {
        //                     Camera.ToggleCamera();
        //                     if (Camera.CameraConnected)
        //                     {
        //                         Camera.SetBlueLevel(1.4);
        //                         //Camera.SetGreenLevel(1.0);
        //                         //Camera.SetRedLevel(1.8);
        //                     }
        //                 },
        //                 () => { return true; });
        //
        //         }
        //         return startCameraCommand;
        //     }
        // }
        //
        // private ICommand connectStageCommand;
        // public ICommand ConnectStageCommand
        // {
        //     get
        //     {
        //         if (connectStageCommand == null)
        //         {
        //             connectStageCommand = new RelayCommand(
        //                 () => ToggleStageConnection(SelectedPort.Name),
        //                 () => { return true; });
        //         }
        //         return connectStageCommand;
        //     }
        // }
        //
        // private ICommand refreshStageCommand;
        // public ICommand RefreshStageCommand
        // {
        //     get
        //     {
        //         if (refreshStageCommand == null)
        //         {
        //             refreshStageCommand = new RelayCommand(
        //                 () => UpdatePortsList(),
        //                 () => { return true; });
        //         }
        //         return refreshStageCommand;
        //     }
        // }
        //
        // private ICommand stageGoToUpperLeftCommand;
        // public ICommand StageGoToUpperLeftCommand
        // {
        //     get
        //     {
        //         if (stageGoToUpperLeftCommand == null)
        //         {
        //             stageGoToUpperLeftCommand = new RelayCommand(
        //                 () => Scanner.GoToTopLeft(),
        //                 () => { return true; });
        //         }
        //         return stageGoToUpperLeftCommand;
        //     }
        // }
        //
        // private ICommand stageGoToLowerRightCommand;
        // public ICommand StageGoToLowerRightCommand
        // {
        //     get
        //     {
        //         if (stageGoToLowerRightCommand == null)
        //         {
        //             stageGoToLowerRightCommand = new RelayCommand(
        //                 () => Scanner.GoToBottomRight(),
        //                 () => { return true; });
        //         }
        //         return stageGoToLowerRightCommand;
        //     }
        // }
        //
        // private ICommand stageHomeCommand;
        // public ICommand StageHomeCommand
        // {
        //     get
        //     {
        //         if (stageHomeCommand == null)
        //         {
        //             stageHomeCommand = new RelayCommand(
        //                 () => Stage.Home(),
        //                 () => { return true; });
        //         }
        //         return stageHomeCommand;
        //     }
        // }
        //
        // private ICommand stageZeroCommand;
        // public ICommand StageZeroCommand
        // {
        //     get
        //     {
        //         if (stageZeroCommand == null)
        //         {
        //             stageZeroCommand = new RelayCommand(
        //                 () => Stage.Zero(),
        //                 () => { return true; });
        //         }
        //         return stageZeroCommand;
        //     }
        // }
        //
        // private ICommand stageForwardCommand;
        // public ICommand StageForwardCommand
        // {
        //     get
        //     {
        //         if (stageForwardCommand == null)
        //         {
        //             stageForwardCommand = new RelayCommand<int>(
        //                 p => Stage.MoveRelative("y", true, Scanner.StepY * p / 1000),
        //                 p => { return true; });
        //         }
        //         return stageForwardCommand;
        //     }
        // }
        //
        // private ICommand stageBackCommand;
        // public ICommand StageBackCommand
        // {
        //     get
        //     {
        //         if (stageBackCommand == null)
        //         {
        //             stageBackCommand = new RelayCommand<int>(
        //                 p => Stage.MoveRelative("y", false, Scanner.StepY * p / 1000),
        //                 p => { return true; });
        //         }
        //         return stageBackCommand;
        //     }
        // }
        //
        // private ICommand stageRightCommand;
        // public ICommand StageRightCommand
        // {
        //     get
        //     {
        //         if (stageRightCommand == null)
        //         {
        //             stageRightCommand = new RelayCommand<int>(
        //                 p => Stage.MoveRelative("x", true, Scanner.StepX * p / 1000),
        //                 p => { return true; });
        //         }
        //         return stageRightCommand;
        //     }
        // }
        //
        // private ICommand stageLeftCommand;
        // public ICommand StageLeftCommand
        // {
        //     get
        //     {
        //         if (stageLeftCommand == null)
        //         {
        //             stageLeftCommand = new RelayCommand<int>(
        //                 p => Stage.MoveRelative("x", false, Scanner.StepX * p / 1000),
        //                 p => { return true; });
        //         }
        //         return stageLeftCommand;
        //     }
        // }
        //
        // private ICommand stageUpCommand;
        // public ICommand StageUpCommand
        // {
        //     get
        //     {
        //         if (stageUpCommand == null)
        //         {
        //             stageUpCommand = new RelayCommand<int>(
        //                 p => Stage.MoveRelative("z", true, (int)p),
        //                 p => { return true; });
        //         }
        //         return stageUpCommand;
        //     }
        // }
        //
        // private ICommand stageDownCommand;
        // public ICommand StageDownCommand
        // {
        //     get
        //     {
        //         if (stageDownCommand == null)
        //         {
        //             stageDownCommand = new RelayCommand<int>(
        //                 p => Stage.MoveRelative("z", false, (int)p),
        //                 p => { return true; });
        //         }
        //         return stageDownCommand;
        //     }
        // }
        //
        // private ICommand stageSetTopLeftCommand;
        // public ICommand StageSetTopLeftCommand
        // {
        //     get
        //     {
        //         if (stageSetTopLeftCommand == null)
        //         {
        //             stageSetTopLeftCommand = new RelayCommand(
        //                 () => Scanner.SetTopLeft(),
        //                 () => { return true; });
        //         }
        //         return stageSetTopLeftCommand;
        //     }
        // }
        //
        // private ICommand stageSetBottomRightCommand;
        // public ICommand StageSetBottomRightCommand
        // {
        //     get
        //     {
        //         if (stageSetBottomRightCommand == null)
        //         {
        //             stageSetBottomRightCommand = new RelayCommand(
        //                 () => Scanner.SetBottomRight(),
        //                 () => { return true; });
        //         }
        //         return stageSetBottomRightCommand;
        //     }
        // }
        //
        // private ICommand scannerSetZCeilingCommand;
        // public ICommand ScannerSetZCeilingCommand
        // {
        //     get
        //     {
        //         if (scannerSetZCeilingCommand == null)
        //         {
        //             scannerSetZCeilingCommand = new RelayCommand(
        //                 () => Scanner.SetZCeiling(),
        //                 () => { return true; });
        //         }
        //         return scannerSetZCeilingCommand;
        //     }
        // }
        //
        // private ICommand scannerSetZFloorCommand;
        // public ICommand ScannerSetZFloorCommand
        // {
        //     get
        //     {
        //         if (scannerSetZFloorCommand == null)
        //         {
        //             scannerSetZFloorCommand = new RelayCommand(
        //                 () => Scanner.SetZFloor(),
        //                 () => { return true; });
        //         }
        //         return scannerSetZFloorCommand;
        //     }
        // }
        //
        // private ICommand changeFolderCommand;
        // public ICommand ChangeFolderCommand
        // {
        //     get
        //     {
        //         if (changeFolderCommand == null)
        //         {
        //             changeFolderCommand = new RelayCommand(
        //                 () => ChangeFolder(),
        //                 () => { return true; });
        //         }
        //         return changeFolderCommand;
        //     }
        // }
        //
        // private ICommand scannerStartCommand;
        // public ICommand ScannerStartCommand
        // {
        //     get
        //     {
        //         if (scannerStartCommand == null)
        //         {
        //             scannerStartCommand = new RelayCommand(
        //                 async () =>
        //                 {
        //                     if (!Scanner.IsScanning)
        //                     {
        //                         SaveSettings();
        //                         await Scanner.Scan();
        //                     }
        //                     else
        //                     {
        //                         Scanner.Stop();
        //                     }
        //                 },
        //                 () => { return true; });
        //         }
        //         return scannerStartCommand;
        //     }
        // }
        //
        // #endregion
    }
}
