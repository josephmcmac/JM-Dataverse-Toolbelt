namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class BigIntFieldMetadata : FieldMetadata
    {
        public BigIntFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
        }

        public BigIntFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.BigInt; }
        }
    }
}