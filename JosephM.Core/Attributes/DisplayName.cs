using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    /// Alternative display name for a class or property
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Property,
        AllowMultiple = false)]
    public class DisplayNameAttribute : Attribute
    {
        public string Label { get; private set; }

        public DisplayNameAttribute(string label)
        {
            Label = label;
        }
    }
}