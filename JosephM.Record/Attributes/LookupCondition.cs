using System;
using JosephM.Record.Query;

namespace JosephM.Record.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class LookupCondition : Attribute
    {
        public string FieldName { get; set; }
        public ConditionType ConditionType { get; set; }
        public object Value { get; set; }

        public LookupCondition(string fieldName, object value)
            : this(fieldName, ConditionType.Equal, value)
        {
        }

        public LookupCondition(string fieldName, ConditionType conditionType, object value)
        {
            Value = value;
            FieldName = fieldName;
            ConditionType = conditionType;
        }

        public Condition ToCondition()
        {
            return new Condition(FieldName, ConditionType, Value);
        }
    }
}
