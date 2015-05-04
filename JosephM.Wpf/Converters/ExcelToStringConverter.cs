#region

using System;
using System.Globalization;
using System.Windows.Data;
using JosephM.Core.FieldType;

#endregion

namespace JosephM.Wpf.Converters
{
    public class ExcelToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return ((ExcelFile) value).FileName;
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty((string) value))
                return null;
            else
                return new ExcelFile((string) value);
        }
    }
}