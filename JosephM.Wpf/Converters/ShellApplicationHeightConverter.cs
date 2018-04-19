using System;
using System.Globalization;
using System.Windows.Data;

namespace JosephM.Wpf.Converters
{
    public class ShellApplicationHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is double)
            {
                var theDouble = (double)value;
                return (theDouble - 40); 
            }
            return (double)500;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implemented.");
        }
    }
}