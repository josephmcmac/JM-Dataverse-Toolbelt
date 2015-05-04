namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class LookupFieldMetadata : FieldMetadata
    {
        public LookupFieldMetadata(string internalName, string label, string referencedRecordType)
            : base(internalName, label)
        {
            ReferencedRecordType = referencedRecordType;
        }

        public LookupFieldMetadata(string recordType, string internalName, string label, string referencedRecordType)
            : base(recordType, internalName, label)
        {
            ReferencedRecordType = referencedRecordType;
        }

        public string ReferencedRecordType { get; private set; }

        public bool DisplayInRelated { get; set; }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Lookup; }
        }
    }
}