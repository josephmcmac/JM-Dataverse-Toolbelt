namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class DecimalFieldMetadata : FieldMetadata
    {
        public DecimalFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
            MinValue = new decimal(-100000000000);
            MaxValue = new decimal(100000000000);
            DecimalPrecision = 10;
        }

        public DecimalFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
            MinValue = new decimal(-100000000000);
            MaxValue = new decimal(100000000000);
            DecimalPrecision = 10;
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Decimal; }
        }
    }
}