namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class EnumerableFieldMetadata : FieldMetadata
    {
        public EnumerableFieldMetadata(string internalName, string label, string type)
            : base(internalName, label)
        {
            EnumeratedType = type;
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Enumerable; }
        }

        public string EnumeratedType { get; set; }
    }
}