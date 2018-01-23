using System;
using System.Linq;
using JosephM.Core.Extentions;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Defines The Property With The Attribute In Context If Any Of The Boolean Property Values Is True
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class PropertyInContextForAny : PropertyInContext
    {
        public string[] BooleanProperties { get; set; }

        public PropertyInContextForAny(params string[] booleanProperties)
        {
            BooleanProperties = booleanProperties;
        }

        public override bool IsInContext(object instance)
        {
            return BooleanProperties.Any(p => (bool)instance.GetPropertyValue(p));
        }
    }
}