using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MicroscopeImaging.Models;

namespace MicroscopeImaging.ViewModels
{
    public class MoveViewModel : ViewModelBase
    {
        public StageBase Stage { get; set; }
        public Scanner Scanner { get; set; }

        private String selectedPort;
        public String SelectedPort
        {
            get => selectedPort;
            set => Set(ref selectedPort, value);
        }

        public MoveViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real"
            }

            Scanner = ModelLocator.Instance.Scanner;

            Stage = ModelLocator.Instance.Stage;
            Stage.RefreshPorts();
            if (Stage.Ports.Count > 0)
            {
                SelectedPort = Stage.Ports[0];
            }
        }

        


        public ICommand connectCommand;
        public ICommand ConnectCommand
        {
            get => connectCommand == null
                ? new RelayCommand(() =>
                {
                    if (Stage.IsConnected) Stage.Disconnect();
                    else Stage.Connect(SelectedPort);
                }, () => true)
                : connectCommand;
        }

        private ICommand stageHomeCommand;
        public ICommand StageHomeCommand
        {
            get
            {
                if (stageHomeCommand == null)
                {
                    stageHomeCommand = new RelayCommand(
                        () => Stage.Home(),
                        () => { return true; });
                }
                return stageHomeCommand;
            }
        }

        private ICommand stageZeroCommand;
        public ICommand StageZeroCommand
        {
            get
            {
                if (stageZeroCommand == null)
                {
                    stageZeroCommand = new RelayCommand(
                        () => Stage.Zero(),
                        () => { return true; });
                }
                return stageZeroCommand;
            }
        }

        private ICommand stageForwardCommand;
        public ICommand StageForwardCommand
        {
            get
            {
                if (stageForwardCommand == null)
                {
                    stageForwardCommand = new RelayCommand<int>(
                        p => Stage.MoveRelative("y", true, p),
                        p => { return true; });
                }
                return stageForwardCommand;
            }
        }

        private ICommand stageBackCommand;
        public ICommand StageBackCommand
        {
            get
            {
                if (stageBackCommand == null)
                {
                    stageBackCommand = new RelayCommand<int>(
                        p => Stage.MoveRelative("y", false, p),
                        p => { return true; });
                }
                return stageBackCommand;
            }
        }

        private ICommand stageRightCommand;
        public ICommand StageRightCommand
        {
            get
            {
                if (stageRightCommand == null)
                {
                    stageRightCommand = new RelayCommand<int>(
                        p => Stage.MoveRelative("x", true, p),
                        p => { return true; });
                }
                return stageRightCommand;
            }
        }

        private ICommand stageLeftCommand;
        public ICommand StageLeftCommand
        {
            get
            {
                if (stageLeftCommand == null)
                {
                    stageLeftCommand = new RelayCommand<int>(
                        p => Stage.MoveRelative("x", false, p),
                        p => { return true; });
                }
                return stageLeftCommand;
            }
        }

        private ICommand stageUpCommand;
        public ICommand StageUpCommand
        {
            get
            {
                if (stageUpCommand == null)
                {
                    stageUpCommand = new RelayCommand<int>(
                        p => Stage.MoveRelative("z", true, p),
                        p => { return true; });
                }
                return stageUpCommand;
            }
        }

        private ICommand stageDownCommand;
        public ICommand StageDownCommand
        {
            get
            {
                if (stageDownCommand == null)
                {
                    stageDownCommand = new RelayCommand<int>(
                        p => Stage.MoveRelative("z", false, p),
                        p => { return true; });
                }
                return stageDownCommand;
            }
        }

        private ICommand scannerGoToBottomLeftCommand;
        public ICommand ScannerGoToBottomLeftCommand
        {
            get
            {
                if (scannerGoToBottomLeftCommand == null)
                {
                    scannerGoToBottomLeftCommand = new RelayCommand(
                        () => Scanner.GoToBottomLeft(),
                        () => { return true; });
                }
                return scannerGoToBottomLeftCommand;
            }
        }

        private ICommand scannerGoToTopLeftCommand;
        public ICommand ScannerGoToTopLeftCommand
        {
            get
            {
                if (scannerGoToTopLeftCommand == null)
                {
                    scannerGoToTopLeftCommand = new RelayCommand(
                        () => Scanner.GoToTopLeft(),
                        () => { return true; });
                }
                return scannerGoToTopLeftCommand;
            }
        }

        private ICommand scannerGoToTopRightCommand;
        public ICommand ScannerGoToTopRightCommand
        {
            get
            {
                if (scannerGoToTopRightCommand == null)
                {
                    scannerGoToTopRightCommand = new RelayCommand(
                        () => Scanner.GoToTopRight(),
                        () => { return true; });
                }
                return scannerGoToTopRightCommand;
            }
        }

        private ICommand scannerGoToBottomRightCommand;
        public ICommand ScannerGoToBottomRightCommand
        {
            get
            {
                if (scannerGoToBottomRightCommand == null)
                {
                    scannerGoToBottomRightCommand = new RelayCommand(
                        () => Scanner.GoToBottomRight(),
                        () => { return true; });
                }
                return scannerGoToBottomRightCommand;
            }
        }

        public ICommand scannerSetBottomLeftCommand;
        public ICommand ScannerSetBottomLeftCommand
        {
            get => scannerSetBottomLeftCommand == null
                ? new RelayCommand(() => Scanner.SetBottomLeft(), () => true)
                : scannerSetBottomLeftCommand;
        }

        public ICommand scannerSetTopRightCommand;
        public ICommand ScannerSetTopRightCommand
        {
            get => scannerSetTopRightCommand == null
                ? new RelayCommand(() => Scanner.SetTopRight(), () => true)
                : scannerSetTopRightCommand;
        }

        public ICommand scannerSetStackFloorCommand;
        public ICommand ScannerSetStackFloorCommand
        {
            get => scannerSetStackFloorCommand == null
                ? new RelayCommand(() => Scanner.SetStackFloor(), () => true)
                : scannerSetStackFloorCommand;
        }
    }
}
