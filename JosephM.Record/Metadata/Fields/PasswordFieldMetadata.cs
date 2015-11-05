namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class PasswordFieldMetadata : FieldMetadata
    {
        public PasswordFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
            MaxLength = 4000;
            TextFormat = TextFormat.Text;
        }

        public PasswordFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
            MaxLength = 4000;
            TextFormat = TextFormat.Text;
        }

        public bool IsPrimaryField { get; set; }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Password; }
        }
    }
}