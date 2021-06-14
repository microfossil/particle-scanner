using System;
using System.Windows.Data;

namespace WPFHelpers.Converters
{
    public class AdditionConverter : IValueConverter
    {
        public double Addend { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDouble(value) + Addend;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.Convert.ToDouble(value) - Addend;
        }
    }
}