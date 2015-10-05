namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class BooleanFieldMetadata : FieldMetadata
    {
        public BooleanFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
        }

        public BooleanFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Boolean; }
        }
    }
}