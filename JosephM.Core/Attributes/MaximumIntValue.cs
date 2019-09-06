using System;

namespace JosephM.Core.Attributes
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Property,
        AllowMultiple = false)]
    public class MaximumIntValue : Attribute
    {
        public int Value { get; set; }

        public MaximumIntValue(int minimumValue)
        {
            Value = minimumValue;
        }
    }
}