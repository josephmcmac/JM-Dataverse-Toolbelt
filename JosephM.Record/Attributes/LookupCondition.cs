using System;
using JosephM.Record.IService;
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
        public bool ValueIsProperty { get; }

        public LookupCondition(string fieldName, object value, bool valueIsProperty = false)
            : this(fieldName, ConditionType.Equal, value)
        {
            ValueIsProperty = valueIsProperty;
        }

        public LookupCondition(string fieldName, ConditionType conditionType, object value)
        {
            Value = value;
            FieldName = fieldName;
            ConditionType = conditionType;
        }

        public Condition ToCondition(IRecord record = null)
        {
            if (ValueIsProperty)
            {
                if (record == null)
                {
                    throw new ArgumentNullException("record", $"Required when {nameof(ValueIsProperty)}");
                }
                return new Condition(FieldName, ConditionType, record.GetField(Value.ToString()));
            }
            else
            {
                return new Condition(FieldName, ConditionType, Value);
            }
        }
    }
}
