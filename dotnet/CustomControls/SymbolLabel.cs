using System;
using System.Windows;
using System.Windows.Controls;

namespace CustomControls
{
    public class SymbolLabel : Label
    {
        static SymbolLabel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SymbolLabel), new FrameworkPropertyMetadata(typeof(SymbolLabel)));
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

        public Thickness SymbolPadding
        {
            get { return (Thickness)GetValue(SymbolPaddingProperty); }
            set { SetValue(SymbolPaddingProperty, value); }
        }

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }



        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register("Symbol",
                typeof(string),
                typeof(SymbolLabel),
                new PropertyMetadata(""));

        public static readonly DependencyProperty SymbolSizeProperty =
            DependencyProperty.Register("SymbolSize",
                typeof(double),
                typeof(SymbolLabel),
                new PropertyMetadata(14.0));

        public static readonly DependencyProperty SymbolPaddingProperty =
            DependencyProperty.Register("SymbolPadding",
                typeof(Thickness),
                typeof(SymbolLabel),
                new PropertyMetadata(new Thickness(0,0,7,0)));

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation",
                typeof(Orientation),
                typeof(SymbolLabel),
                new PropertyMetadata(Orientation.Horizontal));
    }
}
