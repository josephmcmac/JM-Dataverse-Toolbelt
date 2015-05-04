namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class MoneyFieldMetadata : FieldMetadata
    {
        public MoneyFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
            Minimum = -100000000000;
            Maximum = 100000000000;
        }

        public MoneyFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
            Minimum = -100000000000;
            Maximum = 100000000000;
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Money; }
        }

        public double Minimum { get; set; }

        public double Maximum { get; set; }
    }
}