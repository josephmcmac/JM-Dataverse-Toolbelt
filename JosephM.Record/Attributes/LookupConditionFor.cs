using System;

namespace JosephM.Record.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class LookupConditionFor : Attribute
    {
        public string TargetProperty { get; set; }
        public string FieldName { get; set; }

        public LookupConditionFor(string targetProperty, string fieldName)
        {
            TargetProperty = targetProperty;
            FieldName = fieldName;
        }
    }
}
