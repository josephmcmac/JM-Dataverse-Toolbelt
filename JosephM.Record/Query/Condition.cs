using System;
using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Record.IService;

namespace JosephM.Record.Query
{
    public class Condition
    {
        public Condition(string fieldName, ConditionType conditionType, object value)
        {
            FieldName = fieldName;
            ConditionType = conditionType;
            Value = value;
        }

        public Condition(string fieldName, ConditionType conditionType)
        {
            FieldName = fieldName;
            ConditionType = conditionType;
        }

        public string FieldName { get; set; }
        public ConditionType ConditionType { get; set; }
        public object Value { get; set; }

        public bool MeetsCondition(IRecord record)
        {
            var fieldValue = record.GetField(FieldName);
            switch (ConditionType)
            {
                case ConditionType.Like:
                {
                    if (!(Value is string))
                        throw new Exception(string.Format("Expected Value Of Type {0} For Condition Of Type {1}",
                            typeof (string), ConditionType.Like));
                    return fieldValue != null && fieldValue.ToString().ToLower().Like(((string) Value).ToLower());
                }
                case ConditionType.Equal:
                {
                    if (fieldValue == Value)
                        return true;
                    if (fieldValue == null)
                        return Value == null;
                    if (fieldValue is Lookup && Value is string)
                        return ((Lookup) fieldValue).Id.Equals(Value);
                    return Value.Equals(fieldValue);
                }
                case ConditionType.GreaterThan:
                {
                    if (fieldValue == null)
                        return false;
                    if (Value == null)
                        return true;
                    if (fieldValue is IComparable && Value is IComparable)
                        return ((IComparable)fieldValue).CompareTo(Value) > 0;
                    throw new NotSupportedException(string.Format("Type {0} Not Of {1} Type For {2} Operator", fieldValue.GetType(), typeof(IComparable), ConditionType));
                }
                case ConditionType.GreaterEqual:
                {
                    if (fieldValue == null)
                        return Value == null;
                    if (fieldValue is IComparable && Value is IComparable)
                        return ((IComparable)fieldValue).CompareTo(Value) >= 0;
                    throw new NotSupportedException(string.Format("Type {0} Not Of {1} Type For {2} Operator", fieldValue.GetType(), typeof(IComparable), ConditionType));
                }
                case ConditionType.LessEqual:
                {
                    if (Value == null)
                        return true;
                    if (fieldValue is IComparable && Value is IComparable)
                        return ((IComparable)fieldValue).CompareTo(Value) <= 0;
                    throw new NotSupportedException(string.Format("Type {0} Not Of {1} Type For {2} Operator", fieldValue.GetType(), typeof(IComparable), ConditionType));
                }
                case ConditionType.NotNull:
                {
                    return fieldValue.IsEmpty();
                }
            }
            throw new NotSupportedException(string.Format("No Logic Implemented For Condition Type {0}", ConditionType));
        }
    }
}