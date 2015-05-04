using System;
using JosephM.Core.Extentions;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define A Property As Required To Be Non-Empty To Be Valid
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class RequiredProperty : PropertyValidator
    {
        public override bool IsValid(object value, object instance)
        {
            return value.IsNotEmpty();
        }

        public override string GetErrorMessage(string propertyName, object instance)
        {
            return string.Format("{0} must be populated", instance.GetType().GetPropertyDisplayName(propertyName));
        }
    }
}