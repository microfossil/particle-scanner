using System;
using System.Windows.Data;

namespace WPFHelpers.Converters
{
    public class IntegerToColourConverter : IValueConverter
    {
        public string[] Colours =
        {
            "#335df8",
            "#336de6",
            "#337cd2",
            "#3388be",
            "#3492ab",
            "#549a9a",
            "#5fa08a",
            "#65a878",
            "#66af63",
            "#68b74b",
            "#77bc3f",
            "#8bc142",
            "#9bc445",
            "#abc848",
            "#bccc4b",
            "#cbd04f",
            "#dbd452",
            "#ead755",
            "#f7d857",
            "#fdd255",
            "#ffc851",
            "#ffbe4d",
            "#ffb449",
            "#ffaa45",
            "#ffa041",
            "#ff953d",
            "#ff8939",
            "#ff7b35",
            "#ff6c33",
            "#fe5a33"
        //"#cceecc",
        //"#bbdddd",
        //"#ffddaa",
        //"#c6ffff",
        //"#eeccdd",
        //"#c3f8e4",
        //"#ddddee",
        //"#ddeeaa",
        //"#ffccee",
        //"#ffeeff",
        //"#eeccbb",
        //"#ddddcc",
        //"#ccddee",
        //"#ffffe0",
        //"#ffeebb",
        //"#88ffff",
        //"#ddddaa",
        //"#cceeaa",
        //"#44eedd",
        //"#eeeeff",
        //"#88ffee",
        //"#dbdbff",
        //"#aaffbb",
        //"#ddeedd",
        //"#ffccff",
        //"#88ffdd",
        //"#ffffcc",
        //"#ffdd88",
        //"#ffe3db",
        //"#ccddff"
    };

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int idx = System.Convert.ToInt32(value);
            return Colours[idx % Colours.Length];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}