namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class StringFieldMetadata : FieldMetadata
    {
        public StringFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
            MaxLength = 4000;
            TextFormat = TextFormat.Text;
        }

        public StringFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
            MaxLength = 4000;
            TextFormat = TextFormat.Text;
        }

        public bool IsPrimaryField { get; set; }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.String; }
        }
    }
}