using System;
using System.Windows.Data;

namespace WPFHelpers.Converters
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            int returnValue = 0;
            if (parameter is Type)
            {
                returnValue = (int)Enum.Parse((Type)parameter, value.ToString());
            }

            if (returnValue == 0) return false;
            else return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            int intValue = (bool) value ? 1 : 0;
            Enum enumValue = default(Enum);
            if (parameter is Type)
            {
                enumValue = (Enum)Enum.Parse((Type)parameter, intValue.ToString());
            }
            return enumValue;
        }
    }
}