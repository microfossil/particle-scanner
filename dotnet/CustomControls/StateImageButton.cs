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
    ///     <MyNamespace:StateImageButton/>
    ///
    /// </summary>
    public class StateImageButton : Button
    {
        static StateImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(StateImageButton), new FrameworkPropertyMetadata(typeof(StateImageButton)));
        }

        public bool State
        {
            get { return (bool)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        public String TextOn
        {
            get { return (String)GetValue(TextOnProperty); }
            set { SetValue(TextOnProperty, value); }
        }

        public String IconOn
        {
            get { return (String)GetValue(IconOnProperty); }
            set { SetValue(IconOnProperty, value); }
        }

        public String TextOff
        {
            get { return (String)GetValue(TextOffProperty); }
            set { SetValue(TextOffProperty, value); }
        }

        public String IconOff
        {
            get { return (String)GetValue(IconOffProperty); }
            set { SetValue(IconOffProperty, value); }
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State",
                typeof(bool),
                typeof(StateImageButton),
                new PropertyMetadata(false));

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation",
                typeof(Orientation),
                typeof(StateImageButton),
                new PropertyMetadata(Orientation.Vertical));

        public static readonly DependencyProperty TextOnProperty =
            DependencyProperty.Register("TextOn",
            typeof(string),
            typeof(StateImageButton),
            new PropertyMetadata(""));

        public static readonly DependencyProperty IconOnProperty =
            DependencyProperty.Register("IconOn",
            typeof(string),
            typeof(StateImageButton),
            new PropertyMetadata(""));

        public static readonly DependencyProperty TextOffProperty =
            DependencyProperty.Register("TextOff",
                typeof(string),
                typeof(StateImageButton),
                new PropertyMetadata(""));

        public static readonly DependencyProperty IconOffProperty =
            DependencyProperty.Register("IconOff",
                typeof(string),
                typeof(StateImageButton),
                new PropertyMetadata(""));
    }
}
