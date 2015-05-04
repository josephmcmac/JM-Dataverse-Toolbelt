#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace JosephM.Wpf.Converters
{
    public class StringToEnumerableStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? null : String.Join(",", (IEnumerable<string>) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? ((string) value).Split(',') : null;
        }
    }
}