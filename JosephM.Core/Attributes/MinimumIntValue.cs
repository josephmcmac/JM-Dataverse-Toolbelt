using System;

namespace JosephM.Core.Attributes
{
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Property,
        AllowMultiple = false)]
    public class MinimumIntValue : Attribute
    {
        public int Value { get; set; }

        public MinimumIntValue(int minimumValue)
        {
            Value = minimumValue;
        }
    }
}