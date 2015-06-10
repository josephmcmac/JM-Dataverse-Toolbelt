namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class FileRefFieldMetadata : FieldMetadata
    {
        public FileRefFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
        }

        public FileRefFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.FileRef; }
        }
    }
}