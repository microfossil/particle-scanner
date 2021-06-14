using System;
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
    ///     xmlns:MyNamespace="clr-namespace:CustomControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:CustomControls;assembly=CustomControls"
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
    ///     <MyNamespace:LabeledContentControl/>
    ///
    /// </summary>
    public class LabeledContentControl : ContentControl
    {
        static LabeledContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LabeledContentControl), new FrameworkPropertyMetadata(typeof(LabeledContentControl)));
        }

        public String Title
        {
            get { return (String)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public double TitleWidth
        {
            get { return (double)GetValue(TitleWidthProperty); }
            set { SetValue(TitleWidthProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title",
                typeof(string),
                typeof(LabeledContentControl),
                new PropertyMetadata("Title"));

        public static readonly DependencyProperty TitleWidthProperty =
            DependencyProperty.Register("TitleWidth",
                typeof(double),
                typeof(LabeledContentControl),
                new PropertyMetadata(120.0));
    }
}
