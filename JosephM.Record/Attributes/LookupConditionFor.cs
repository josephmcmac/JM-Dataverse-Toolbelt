using System;

namespace JosephM.Record.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class LookupCondition : Attribute
    {
        public string FieldName { get; set; }
        public object Value { get; set; }

        public LookupCondition(string fieldName, object value)
        {
            Value = value;
            FieldName = fieldName;
        }
    }
}
