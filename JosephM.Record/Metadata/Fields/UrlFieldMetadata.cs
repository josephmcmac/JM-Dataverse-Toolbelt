using System;

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class UrlFieldMetadata : FieldMetadata
    {
        public UrlFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
        }

        public UrlFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
        }

        public override RecordFieldType FieldType
        {
            get
            {
                return RecordFieldType.Url;
            }
        }
    }
}