using JosephM.Record.Attributes;
using JosephM.Record.Metadata;

namespace JosephM.Record.Query
{
    public enum ConditionType
    {
        Equal,
        NotEqual,
        Null,
        NotNull,
        [ValidForFieldTypes(RecordFieldType.BigInt, RecordFieldType.Date, RecordFieldType.Decimal, RecordFieldType.Double, RecordFieldType.Integer, RecordFieldType.Money)]
        GreaterThan,
        [ValidForFieldTypes(RecordFieldType.BigInt, RecordFieldType.Date, RecordFieldType.Decimal, RecordFieldType.Double, RecordFieldType.Integer, RecordFieldType.Money)]
        LessThan,
        [ValidForFieldTypes(RecordFieldType.BigInt, RecordFieldType.Date, RecordFieldType.Decimal, RecordFieldType.Double, RecordFieldType.Integer, RecordFieldType.Money)]
        GreaterEqual,
        [ValidForFieldTypes(RecordFieldType.BigInt, RecordFieldType.Date, RecordFieldType.Decimal, RecordFieldType.Double, RecordFieldType.Integer, RecordFieldType.Money)]
        LessEqual,
        [ValidForFieldTypes(RecordFieldType.String, RecordFieldType.Memo)]
        BeginsWith,
        [ValidForFieldTypes(RecordFieldType.String, RecordFieldType.Memo)]
        Like,
        //this actually just a fake to prevent it displaying in UI at all
        //as haven't implemented multiselect
        [ValidForFieldTypes(RecordFieldType.FileRef)]
        In
    }
}