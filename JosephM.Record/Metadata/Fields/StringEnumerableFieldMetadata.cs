namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class StringEnumerableFieldMetadata : FieldMetadata
    {
        public StringEnumerableFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
            MaxLength = 4000;
            TextFormat = TextFormat.Text;
        }

        public StringEnumerableFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
            MaxLength = 4000;
            TextFormat = TextFormat.Text;
        }

        public int MaxLength { get; set; }
        public TextFormat TextFormat { get; set; }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.StringEnumerable; }
        }
    }
}