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
    ///     <MyNamespace:HorizontalImageButton/>
    ///
    /// </summary>
    public class SymbolButton : Button
    {
        static SymbolButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SymbolButton), new FrameworkPropertyMetadata(typeof(SymbolButton)));
        }

        // Plain button
        public String Text
        {
            get { return (String)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public String Symbol
        {
            get { return (String)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public double SymbolSize
        {
            get { return (double)GetValue(SymbolSizeProperty); }
            set { SetValue(SymbolSizeProperty, value); }
        }

        ////Activate button
        //public bool IsActive
        //{
        //    get { return (bool)GetValue(IsActiveProperty); }
        //    set { SetValue(IsActiveProperty, value); }
        //}

        //public String ActiveText
        //{
        //    get { return (String)GetValue(ActiveTextProperty); }
        //    set { SetValue(ActiveTextProperty, value); }
        //}

        //public String ActiveSymbol
        //{
        //    get { return (String)GetValue(ActiveSymbolProperty); }
        //    set { SetValue(ActiveSymbolProperty, value); }
        //}

        //public String DisabledText
        //{
        //    get { return (String)GetValue(DisabledTextProperty); }
        //    set { SetValue(DisabledTextProperty, value); }
        //}

        //public String DisabledSymbol
        //{
        //    get { return (String)GetValue(DisabledSymbolProperty); }
        //    set { SetValue(DisabledSymbolProperty, value); }
        //}

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
                typeof(string),
                typeof(SymbolButton),
                new PropertyMetadata(""));

        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register("Symbol",
                typeof(string),
                typeof(SymbolButton),
                new PropertyMetadata(""));

        public static readonly DependencyProperty SymbolSizeProperty =
            DependencyProperty.Register("SymbolSize",
                typeof(double),
                typeof(SymbolButton),
                new PropertyMetadata(14.0));

        //public static readonly DependencyProperty IsActiveProperty =
        //    DependencyProperty.Register("IsActive",
        //        typeof(bool),
        //        typeof(SymbolButton),
        //        new PropertyMetadata(false));

        //public static readonly DependencyProperty ActiveTextProperty =
        //    DependencyProperty.Register("ActiveText",
        //    typeof(string),
        //    typeof(SymbolButton),
        //    new PropertyMetadata(""));
        
        //public static readonly DependencyProperty ActiveSymbolProperty =
        //    DependencyProperty.Register("ActiveSymbol",
        //        typeof(string),
        //        typeof(SymbolButton),
        //        new PropertyMetadata(""));

        //public static readonly DependencyProperty DisabledTextProperty =
        //    DependencyProperty.Register("DisabledText",
        //        typeof(string),
        //        typeof(SymbolButton),
        //        new PropertyMetadata(""));

        //public static readonly DependencyProperty DisabledSymbolProperty =
        //    DependencyProperty.Register("DisabledSymbol",
        //        typeof(string),
        //        typeof(SymbolButton),
        //        new PropertyMetadata(""));
    }
}
