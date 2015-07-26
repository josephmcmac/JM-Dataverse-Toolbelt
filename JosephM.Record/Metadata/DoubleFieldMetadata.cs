namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class DoubleFieldMetadata : FieldMetadata
    {
        public DoubleFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
            Minimum = double.MinValue;
            Maximum = double.MaxValue;
            DecimalPrecision = 10;
        }

        public DoubleFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
            Minimum = double.MinValue;
            Maximum = double.MaxValue;
            DecimalPrecision = 10;
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Double; }
        }

        public int DecimalPrecision { get; set; }

        public double Minimum { get; set; }

        public double Maximum { get; set; }
    }
}