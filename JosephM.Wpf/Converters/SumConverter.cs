using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace JosephM.Wpf.Converters
{
    public class SumConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values
                .Where(x => x is double)
                .Cast<double>()
                .Sum();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
