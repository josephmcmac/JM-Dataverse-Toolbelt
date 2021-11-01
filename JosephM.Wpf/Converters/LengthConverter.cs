using System;
using System.Globalization;
using System.Windows.Data;

namespace JosephM.Wpf.Converters
{
    public class LengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double result = 100;
            if (parameter == null)
            {
                parameter = "1";
            }
            if (value is double d && parameter != null)
            {
                result = d * double.Parse(parameter.ToString(), CultureInfo.InvariantCulture);
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implemented.");
        }
    }
}