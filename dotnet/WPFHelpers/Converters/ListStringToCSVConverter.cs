using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace WPFHelpers.Converters
{
    public class ListStringToCsvConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable<String> strings = (IEnumerable<string>) value;
            int idx = 0;
            StringBuilder sb = new StringBuilder();
            foreach (string str in strings)
            {
                if (idx > 0) sb.Append(", ");
                sb.Append(str);
                idx++;
            }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}