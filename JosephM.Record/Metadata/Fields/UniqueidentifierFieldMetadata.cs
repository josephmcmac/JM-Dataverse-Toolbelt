namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class UniqueidentifierFieldMetadata : FieldMetadata
    {
        public UniqueidentifierFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
        }

        public UniqueidentifierFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Uniqueidentifier; }
        }
    }
}