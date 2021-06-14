using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MicroscopeImaging.Models;

namespace MicroscopeImaging.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        public StageBase Stage { get; set; }
        public BaslerCamera Camera { get; set; }
        private Calibrator calibrator;

        public SettingsViewModel()
        {
            Stage = ModelLocator.Instance.Stage;
            Camera = ModelLocator.Instance.Camera;

            calibrator = new Calibrator(Stage, Camera);
        }


        private ICommand calibrateResolutionCommand;

        public ICommand CalibrateResolutionCommand
        {
            get => calibrateResolutionCommand == null
                ? new RelayCommand(() =>
                {
                    calibrator.CalibrateResolution();
                }, () => true)
                : calibrateResolutionCommand;
        }
    }
}
