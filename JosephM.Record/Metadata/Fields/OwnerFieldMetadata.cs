namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class OwnerFieldMetadata : FieldMetadata
    {
        public OwnerFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
        }

        public OwnerFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
        }

        public string ReferencedRecordType { get; private set; }

        public bool DisplayInRelated { get; set; }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Owner; }
        }

        public CascadeBehaviour OnDeleteBehaviour { get; set; }
    }
}