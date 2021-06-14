using System.Windows;
using System.Windows.Media;

namespace CustomControls.Helpers
{
    public static class ControlHelper
    {
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.RegisterAttached(
                "IsActive",
                typeof(bool),
                typeof(ControlHelper),
                new PropertyMetadata(true));

        public static object GetIsActive(UIElement element)
        {
            return element.GetValue(IsActiveProperty);
        }

        public static void SetIsActive(UIElement element, Color value)
        {
            element.SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.RegisterAttached(
                "Header",
                typeof(object),
                typeof(ControlHelper),
                new FrameworkPropertyMetadata("header"));

        public static object GetHeader(UIElement element)
        {
            return element.GetValue(HeaderProperty);
        }

        public static void SetHeader(UIElement element, object value)
        {
            element.SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.RegisterAttached(
                "Body",
                typeof(object),
                typeof(ControlHelper),
                new FrameworkPropertyMetadata("body"));

        public static object GetBody(UIElement element)
        {
            return element.GetValue(BodyProperty);
        }

        public static void SetBody(UIElement element, object value)
        {
            element.SetValue(BodyProperty, value);
        }
    }
}
