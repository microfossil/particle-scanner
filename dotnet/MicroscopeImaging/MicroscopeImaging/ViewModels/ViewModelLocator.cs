/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:MicroscopeImaging"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace MicroscopeImaging.ViewModels
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}

            // SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<MoveViewModel>();
            SimpleIoc.Default.Register<CameraViewModel>();
            SimpleIoc.Default.Register<SettingsViewModel>();
            SimpleIoc.Default.Register<RunViewModel>();
        }

        // public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public MoveViewModel MoveViewModel => ServiceLocator.Current.GetInstance<MoveViewModel>();
        public CameraViewModel CameraViewModel => ServiceLocator.Current.GetInstance<CameraViewModel>();
        public SettingsViewModel SettingsViewModel => ServiceLocator.Current.GetInstance<SettingsViewModel>();
        public RunViewModel RunViewModel => ServiceLocator.Current.GetInstance<RunViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}