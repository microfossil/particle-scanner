using System.Windows;
using System.Windows.Media;

namespace CustomControls.Helpers
{
    public static class SolidColorBrushShadeHelper
    {
        public static readonly DependencyProperty ShadeColourProperty =
            DependencyProperty.RegisterAttached(
                "ShadeColor",
                typeof(Color),
                typeof(SolidColorBrushShadeHelper),
                new PropertyMetadata(Colors.Transparent, OnShadeColorChanged));

        public static Color GetShadeColor(SolidColorBrush brush)
        {
            return (Color)brush.GetValue(ShadeColourProperty);
        }

        public static void SetShadeColor(SolidColorBrush brush, Color value)
        {
            brush.SetValue(ShadeColourProperty, value);
        }
        
        private static void OnShadeColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var brush = (SolidColorBrush)d;
            brush.Color = GetShadeColor((Color)e.NewValue, GetShadeAmount(brush));
        }

        public static readonly DependencyProperty ShadeAmountProperty =
            DependencyProperty.RegisterAttached(
                "ShadeAmount",
                typeof(double),
                typeof(SolidColorBrushShadeHelper),
                new PropertyMetadata(0.0, OnShadeAmountChanged));

        public static double GetShadeAmount(SolidColorBrush brush)
        {
            return (double)brush.GetValue(ShadeAmountProperty);
        }

        public static void SetShadeAmount(SolidColorBrush brush, double value)
        {
            brush.SetValue(ShadeAmountProperty, value);
        }

        private static void OnShadeAmountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var brush = (SolidColorBrush)d;
            brush.Color = GetShadeColor(GetShadeColor(brush), (double)e.NewValue);
        }

        private static Color GetShadeColor(Color color, double amount)
        {
            double red = color.R;
            double green = color.G;
            double blue = color.B;
            red = (255 - red) * amount + red;
            green = (255 - green) * amount + green;
            blue = (255 - blue) * amount + blue;
            return Color.FromRgb((byte)(red), (byte)(green), (byte)(blue));
        }
    }
}
