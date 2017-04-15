using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define An Alternative Display Name For A Class Type Through The TypeEntentions.GetDisplayName Method
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class DisplayNameForPropertyValueAttribute : Attribute
    {
        public string Property { get; private set; }
        public object Value { get; private set; }
        public string Label { get; private set; }

        public DisplayNameForPropertyValueAttribute(string property, object value, string label)
        {
            Property = property;
            Value = value;
            Label = label;
        }
    }
}