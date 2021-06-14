using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ModernWpf.Controls;

namespace MicroscopeImaging.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            NavigationView.SelectedItem = FirstNavItem;
        }

        private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            if (selectedItem != null)
            {
                string selectedItemTag = (string) selectedItem.Tag;
                Debug.WriteLine(selectedItemTag);
                // string pageName = "SamplesCommon.SamplePages." + selectedItemTag;
                Type pageType = typeof(MainView).Assembly.GetType("MicroscopeImaging.Views." + selectedItemTag + "Page");
                ContentFrame.Navigate(pageType);
            }
        }
    }
}
