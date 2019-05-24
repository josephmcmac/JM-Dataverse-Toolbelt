using System;
using System.Collections;
using JosephM.Core.Extentions;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define A Property As Required To Be Non-Empty To Be Valid
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class RequiredPropertyValue : PropertyValidator
    {
        private object Value { get; }
        private string Message { get; }

        public RequiredPropertyValue(object value, string message)
        {
            Value = value;
            Message = message;
        }
        public override bool IsValid(object value)
        {
            return Value.Equals(value);
        }

        public override string GetErrorMessage(string propertyLabel)
        {
            return Message;
        }
    }
}