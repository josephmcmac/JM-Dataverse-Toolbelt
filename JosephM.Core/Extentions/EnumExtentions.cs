#region

using System;
using System.ComponentModel;

#endregion

namespace JosephM.Core.Extentions
{
    public static class EnumExtentions
    {
        /// <summary>
        ///     Returns Either A Display Name For The Enum Option Using Either The Description Attribute Or Name Split
        /// </summary>
        public static string GetDisplayString(this Enum enumValue)
        {
            //got error here once when enum not nullable in object and enums started at 1
            //so try either change/remove enum explicit integrers and/or change to nullable property
            var fi = enumValue.GetType().GetField(enumValue.ToString());
            var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof (DescriptionAttribute), false);
            return attributes.Length > 0
                ? attributes[0].Description
                : enumValue.ToString().SplitCamelCase();
        }
    }
}