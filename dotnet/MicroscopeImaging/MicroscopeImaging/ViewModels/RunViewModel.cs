using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AsyncAwaitBestPractices.MVVM;
using Emgu.CV;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using MicroscopeImaging.Models;

namespace MicroscopeImaging.ViewModels
{
    public class RunViewModel : ViewModelBase
    {
        public StageBase Stage { get; set; }
        public Scanner Scanner { get; set; }

        public RunViewModel()
        {
            Scanner = ModelLocator.Instance.Scanner;
        }

        public IAsyncCommand scannerStartCommand;
        public IAsyncCommand ScannerStartCommand
        {
            get => scannerStartCommand == null
                ? new AsyncCommand(() => Scanner.Scan(), p => true)
                : scannerStartCommand;
        }
    }
}
