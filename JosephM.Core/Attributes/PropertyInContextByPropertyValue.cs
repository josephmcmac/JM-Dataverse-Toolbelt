using System;
using JosephM.Core.Extentions;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Defines The Property With The Attribute In Context If The Property With A Given Name Has The Given Value
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
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
            if (propertyValue == null)
                return false;
            else
            {
                return propertyValue.Equals(ValidValue);
            }
        }
    }
}