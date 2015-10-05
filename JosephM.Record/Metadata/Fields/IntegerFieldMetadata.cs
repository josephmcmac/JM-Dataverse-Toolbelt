using System;

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class IntegerFieldMetadata : FieldMetadata
    {
        public IntegerFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
            MinValue = Int32.MinValue;
            MaxValue = Int32.MaxValue;
        }

        public IntegerFieldMetadata(string internalName, string label, int minimumValue, int maximumValue)
            : base(internalName, label)
        {
            MinValue = minimumValue;
            MaxValue = maximumValue;
        }

        public IntegerFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
            MinValue = Int32.MinValue;
            MaxValue = Int32.MaxValue;
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Integer; }
        }
    }
}