using System;
using System.Globalization;
using System.Windows.Data;

namespace WPFHelpers.Converters
{
    public class BoolToObjectConverter : IValueConverter
    {
        public object TrueObject { get; set; }
        public object FalseObject { get; set; }
        public object NullObject { get; set; }

        public BoolToObjectConverter()
        {

        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //Console.WriteLine("! BOOL CONVERTER !   " + value);
            if (value == null) return NullObject;
            bool boolValue = true;
            bool isBool = true;
            try
            {
                boolValue = (bool)value;
            }
            catch
            {
                isBool = false;
            }

            if (!isBool) return NullObject;
            //Console.WriteLine("! BOOL CONVERTER !   " + TrueObject);
            //Console.WriteLine("! BOOL CONVERTER !   " + FalseObject);
            return boolValue ? TrueObject : FalseObject;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        //public override object ProvideValue(IServiceProvider serviceProvider)
        //{
        //    return null;
        //}
    }
}
