using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace JosephM.Wpf.Converters
{
    public class ScrollWhereGreaterThanOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = ScrollBarVisibility.Hidden;
            if (value is int)
                result = ((int)value) > 1 ? ScrollBarVisibility.Auto : ScrollBarVisibility.Hidden;
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implemented.");
        }
    }
}