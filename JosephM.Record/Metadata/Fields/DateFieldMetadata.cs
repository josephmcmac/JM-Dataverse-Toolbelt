namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class DateFieldMetadata : FieldMetadata
    {
        public DateFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
        }

        public DateFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Date; }
        }
    }
}