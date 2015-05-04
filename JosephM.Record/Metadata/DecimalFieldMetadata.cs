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
            Minimum = new decimal(-100000000000);
            Maximum = new decimal(100000000000);
        }

        public DecimalFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
            Minimum = new decimal(-100000000000);
            Maximum = new decimal(100000000000);
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Decimal; }
        }

        public decimal Minimum { get; set; }

        public decimal Maximum { get; set; }
    }
}