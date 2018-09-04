#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

#endregion

namespace JosephM.Wpf.Converters
{
    public class StringTruncateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(parameter is int && value is string)
            {
                var maxChars = (int)parameter;
                var theString = (string)value;
                if (theString.Length > maxChars)
                    theString = theString.Substring(0, maxChars - 2) + "...";
                return theString;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}