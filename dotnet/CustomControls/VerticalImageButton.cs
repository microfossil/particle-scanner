using System.Windows;
using System.Windows.Controls;

namespace CustomControls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ScannieWPF"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ScannieWPF;assembly=ScannieWPF"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class VerticalImageButton : Button
    {
        static VerticalImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VerticalImageButton), new FrameworkPropertyMetadata(typeof(VerticalImageButton)));
        }

        public bool State
        {
            get { return (bool)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State",
            typeof(bool), typeof(VerticalImageButton), new PropertyMetadata(null));

        public ControlTemplate IconOn
        {
            get { return (ControlTemplate)GetValue(IconOnProperty); }
            set { SetValue(IconOnProperty, value); }
        }
        public static readonly DependencyProperty IconOnProperty =
            DependencyProperty.Register("IconOn",
            typeof(ControlTemplate), typeof(VerticalImageButton), new PropertyMetadata(null));

        public ControlTemplate IconOff
        {
            get { return (ControlTemplate)GetValue(IconOffProperty); }
            set { SetValue(IconOffProperty, value); }
        }
        public static readonly DependencyProperty IconOffProperty =
            DependencyProperty.Register("IconOff",
            typeof(ControlTemplate), typeof(VerticalImageButton), new PropertyMetadata(null));

        public string ContentOn
        {
            get { return (string)GetValue(ContentOnProperty); }
            set { SetValue(ContentOnProperty, value); }
        }
        public static readonly DependencyProperty ContentOnProperty =
            DependencyProperty.Register("ContentOn",
            typeof(string), typeof(VerticalImageButton), new PropertyMetadata(null));

        public string ContentOff
        {
            get { return (string)GetValue(ContentOffProperty); }
            set { SetValue(ContentOffProperty, value); }
        }
        public static readonly DependencyProperty ContentOffProperty =
            DependencyProperty.Register("ContentOff",
            typeof(string), typeof(VerticalImageButton), new PropertyMetadata(null));
    }
}
