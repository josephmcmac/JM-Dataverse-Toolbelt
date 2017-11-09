using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Defines The Property With The Attribute In Context If The Property With A Given Name Has The Given Value
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class PropertyInContextByPropertyValue : PropertyInContext
    {
        public string PropertyDependency { get; set; }
        public object ValidValue { get; set; }

        public PropertyInContextByPropertyValue(string propertyDependency, object validValue)
        {
            PropertyDependency = propertyDependency;
            ValidValue = validValue;
        }

        public override bool IsInContext(object instance)
        {
            var propertyValue = instance.GetPropertyValue(PropertyDependency);
            return InContextMatch(propertyValue, ValidValue);
        }

        public static bool InContextMatch(object propertyValue, object validValue)
        {
            if (propertyValue == null)
                return false;
            else
            {
                if (propertyValue is Lookup && validValue is string && ((Lookup)propertyValue).Name.ToLower() == validValue.ToString().ToLower())
                    return true;
                return propertyValue.Equals(validValue);
            }
        }
    }
}