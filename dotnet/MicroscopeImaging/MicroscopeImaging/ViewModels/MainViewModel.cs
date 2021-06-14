using GalaSoft.MvvmLight;

namespace MicroscopeImaging.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real"
            }
        }
    }
}