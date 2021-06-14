using System;
using System.Windows;
using System.Windows.Controls;

namespace CustomControls
{
    public class SymbolControlBase : Control
    {
        static SymbolControlBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SymbolControlBase), new FrameworkPropertyMetadata(typeof(SymbolControlBase)));
        }

        //
        // Header
        //
        public String Header
        {
            get { return (String)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
        public double HeaderSize
        {
            get { return (double)GetValue(HeaderSizeProperty); }
            set { SetValue(HeaderSizeProperty, value); }
        }
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header",
                typeof(string),
                typeof(SymbolControlBase),
                new PropertyMetadata(""));
        public static readonly DependencyProperty HeaderSizeProperty =
            DependencyProperty.Register("HeaderSize",
                typeof(double),
                typeof(SymbolControlBase),
                new PropertyMetadata(12.0));

        //
        // Symbol
        // 
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
        public static readonly DependencyProperty SymbolProperty =
            DependencyProperty.Register("Symbol",
                typeof(string),
                typeof(SymbolControlBase),
                new PropertyMetadata(""));

        public static readonly DependencyProperty SymbolSizeProperty =
            DependencyProperty.Register("SymbolSize",
                typeof(double),
                typeof(SymbolControlBase),
                new PropertyMetadata(14.0));
    }
}
