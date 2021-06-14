using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CustomControls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MachineImaging.Controls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MachineImaging.Controls;assembly=MachineImaging.Controls"
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
    ///     <MyNamespace:ImageButton/>
    ///
    /// </summary>
    public class ToggleImageDescriptionButton : ToggleButton
    {
        static ToggleImageDescriptionButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleImageDescriptionButton), new FrameworkPropertyMetadata(typeof(ToggleImageDescriptionButton)));
        }

        public String Text
        {
            get { return (String)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public String Description
        {
            get { return (String)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public String Icon
        {
            get { return (String)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public int IconSize
        {
            get { return (int)GetValue(IconSizeProperty); }
            set { SetValue(IconSizeProperty, value); }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation",
                typeof(Orientation),
                typeof(ToggleImageDescriptionButton),
                new PropertyMetadata(Orientation.Vertical));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
            typeof(string),
            typeof(ToggleImageDescriptionButton),
            new PropertyMetadata(""));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description",
                typeof(string),
                typeof(ToggleImageDescriptionButton),
                new PropertyMetadata(""));

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon",
            typeof(string),
            typeof(ToggleImageDescriptionButton),
            new PropertyMetadata(""));

        public static readonly DependencyProperty IconSizeProperty =
            DependencyProperty.Register("IconSize",
                typeof(int),
                typeof(ToggleImageDescriptionButton),
                new PropertyMetadata(18));
    }
}
