namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class ExcelFileFieldMetadata : FieldMetadata
    {
        public ExcelFileFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
        }

        public ExcelFileFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.ExcelFile; }
        }
    }
}