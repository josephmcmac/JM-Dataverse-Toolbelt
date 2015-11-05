namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class MemoFieldMetadata : FieldMetadata
    {
        public MemoFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
            MaxLength = 4000;
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Memo; }
        }
    }
}