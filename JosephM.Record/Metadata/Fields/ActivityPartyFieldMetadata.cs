namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class ActivityPartyFieldMetadata : FieldMetadata
    {
        public ActivityPartyFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.ActivityParty; }
        }
    }
}