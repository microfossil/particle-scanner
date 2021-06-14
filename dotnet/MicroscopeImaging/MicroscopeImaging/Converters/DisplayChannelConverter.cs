using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;
using MicroscopeImaging.ViewModels;

namespace MicroscopeImaging.Converters
{
    public class DisplayChannelConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var channelType = (CameraViewModel.ChannelType) value;
            switch (channelType)
            {
                case CameraViewModel.ChannelType.Normal: return 0;
                case CameraViewModel.ChannelType.Red: return 1;
                case CameraViewModel.ChannelType.Green: return 2;
                case CameraViewModel.ChannelType.Blue: return 3;
                default: return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var channelIndex = (int) value;
            switch (channelIndex)
            {
                case 0: return CameraViewModel.ChannelType.Normal;
                case 1: return CameraViewModel.ChannelType.Red;
                case 2: return CameraViewModel.ChannelType.Green;
                case 3: return CameraViewModel.ChannelType.Blue;
                default: return CameraViewModel.ChannelType.Normal;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
