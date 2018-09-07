﻿using JosephM.Application.ViewModel.Shared;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace JosephM.Wpf.Converters
{
    /// <summary>
    ///     Converts horizontal type
    /// </summary>
    public class AlignHorizontalConverter
        : IValueConverter
    {
        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value != null)
            {
                if (value.Equals(HorizontalJustify.Left))
                    return HorizontalAlignment.Left;
                if (value.Equals(HorizontalJustify.Middle))
                    return HorizontalAlignment.Center;
                if (value.Equals(HorizontalJustify.Right))
                    return HorizontalAlignment.Right;
            }
            return HorizontalAlignment.Left;
        }

        /// <summary>
        ///     Converts a value.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        ///     A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}