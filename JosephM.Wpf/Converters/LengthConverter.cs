using System;
using System.Globalization;
using System.Windows.Data;

namespace JosephM.Wpf.Converters
{
    public class LengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = (double) 100;
            if (parameter == null)
                parameter = "1";
            if (value is double && parameter != null)
                result = (double) value*double.Parse(parameter.ToString());
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implemented.");
        }
    }
}