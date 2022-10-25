using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JosephM.Wpf.Converters
{
    public class BooleanToCenterAlignConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return TextAlignment.Center;
            }
            return TextAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}