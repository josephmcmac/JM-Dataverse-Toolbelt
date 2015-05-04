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
            Minimum = Int32.MinValue;
            Maximum = Int32.MaxValue;
        }

        public IntegerFieldMetadata(string internalName, string label, int minimumValue, int maximumValue)
            : base(internalName, label)
        {
            Minimum = minimumValue;
            Maximum = maximumValue;
        }

        public IntegerFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
            Minimum = Int32.MinValue;
            Maximum = Int32.MaxValue;
        }

        public int Minimum { get; set; }
        public int Maximum { get; set; }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Integer; }
        }
    }
}