using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
    ///     <MyNamespace:BooleanIndicator/>
    ///
    /// </summary>
    public class BooleanIndicator : Label
    {
        static BooleanIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BooleanIndicator), new FrameworkPropertyMetadata(typeof(BooleanIndicator)));
        }

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public string Symbol
        {
            get { return (string)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }
        public double SymbolSize
        {
            get { return (double)GetValue(SymbolSizeProperty); }
            set { SetValue(SymbolSizeProperty, value); }
        }
        public SolidColorBrush ActiveBrush
        {
            get { return (SolidColorBrush)GetValue(ActiveBrushProperty); }
            set { SetValue(ActiveBrushProperty, value); }
        }

        public SolidColorBrush InactiveBrush
        {
            get { return (SolidColorBrush)GetValue(InactiveBrushProperty); }
            set { SetValue(InactiveBrushProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive",
                typeof(bool),
                typeof(BooleanIndicator),
                new PropertyMetadata(false, new PropertyChangedCallback(ActiveChanged)));

        private static void ActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            bool state = (bool)d.GetValue(IsActiveProperty);
            ((BooleanIndicator) d).UpdateStates(state);
        }

        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register("Symbol",
                typeof(string),
                typeof(BooleanIndicator),
                new PropertyMetadata("\uE171"));

        public static readonly DependencyProperty SymbolSizeProperty =
            DependencyProperty.Register("SymbolSize",
                typeof(double),
                typeof(BooleanIndicator),
                new PropertyMetadata(14.0));

        public static readonly DependencyProperty ActiveBrushProperty =
            DependencyProperty.Register("ActiveBrush",
                typeof(SolidColorBrush),
                typeof(BooleanIndicator),
                new UIPropertyMetadata(Brushes.DarkGreen));

        public static readonly DependencyProperty InactiveBrushProperty =
            DependencyProperty.Register("InactiveBrush",
                typeof(SolidColorBrush),
                typeof(BooleanIndicator),
                new UIPropertyMetadata(Brushes.LightGreen));
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (IsActive) VisualStateManager.GoToState(this, "True", true);
        }

        public void UpdateStates(bool state)
        {
            //Debug.WriteLine("BOLL STATE" + state);
            if (state) VisualStateManager.GoToState(this, "True", true);
            else VisualStateManager.GoToState(this, "False", true);

            //const int duration = 10;
            //Storyboard updateStoryboard = new Storyboard();
            //if (state)
            //{
            //    ColorAnimation animation = new ColorAnimation(ActiveBrush.Color, TimeSpan.FromMilliseconds(duration));
            //    Storyboard.SetTargetName(animation, "border");
            //    Storyboard.SetTargetProperty(animation, new PropertyPath("(Border.Background).(SolidColorBrush.Color)"));
            //    updateStoryboard.Children.Add(animation);
            //    animation = new ColorAnimation(ActiveBrush.Color, TimeSpan.FromMilliseconds(duration));
            //    Storyboard.SetTargetName(animation, "border");
            //    Storyboard.SetTargetProperty(animation, new PropertyPath("(Border.BorderBrush).(SolidColorBrush.Color)"));
            //    updateStoryboard.Children.Add(animation);
            //    animation = new ColorAnimation(Colors.White, TimeSpan.FromMilliseconds(duration));
            //    Storyboard.SetTargetName(animation, "symbol");
            //    Storyboard.SetTargetProperty(animation, new PropertyPath("(Foreground).(SolidColorBrush.Color)"));
            //    updateStoryboard.Children.Add(animation);
            //}
            //else
            //{
            //    ColorAnimation animation = new ColorAnimation(Colors.Transparent, TimeSpan.FromMilliseconds(duration));
            //    Storyboard.SetTargetName(animation, "border");
            //    Storyboard.SetTargetProperty(animation, new PropertyPath("(Border.Background).(SolidColorBrush.Color)"));
            //    updateStoryboard.Children.Add(animation);
            //    animation = new ColorAnimation(InactiveBrush.Color, TimeSpan.FromMilliseconds(duration));
            //    Storyboard.SetTargetName(animation, "border");
            //    Storyboard.SetTargetProperty(animation, new PropertyPath("(Border.BorderBrush).(SolidColorBrush.Color)"));
            //    updateStoryboard.Children.Add(animation);
            //    animation = new ColorAnimation(InactiveBrush.Color, TimeSpan.FromMilliseconds(duration));
            //    Storyboard.SetTargetName(animation, "symbol");
            //    Storyboard.SetTargetProperty(animation, new PropertyPath("(Foreground).(SolidColorBrush.Color)"));
            //    updateStoryboard.Children.Add(animation);
            //}
            //updateStoryboard.Begin(this, this.Template);
        }
    }
}
