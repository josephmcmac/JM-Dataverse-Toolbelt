using System;

namespace JosephM.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class UsePicklistAttribute : Attribute
    {
        public UsePicklistAttribute()
        {

        }

        public UsePicklistAttribute(params string[] overrideDisplayField)
        {
            OverrideDisplayField = overrideDisplayField;
        }

        public string[] OverrideDisplayField { get; set; } = new string[0];
    }
}
