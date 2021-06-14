using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;

namespace MicroscopeImaging.Models
{
    public class ModelLocator
    {
        private static ModelLocator _instance;
        public static ModelLocator Instance => _instance ?? (_instance = new ModelLocator());

        public ModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<EnderStage>();
            SimpleIoc.Default.Register<Scanner>();
            SimpleIoc.Default.Register<BaslerCamera>();
        }
        
        public StageBase Stage => ServiceLocator.Current.GetInstance<EnderStage>();
        public Scanner Scanner => ServiceLocator.Current.GetInstance<Scanner>();
        public BaslerCamera Camera => ServiceLocator.Current.GetInstance<BaslerCamera>();
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}
