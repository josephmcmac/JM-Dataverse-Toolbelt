using System;

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class EnumerableFieldMetadata : FieldMetadata
    {
        public EnumerableFieldMetadata(string internalName, string label, string enumeratedType)
            : base(internalName, label)
        {
            EnumeratedTypeQualifiedName = enumeratedType;
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Enumerable; }
        }

        public string EnumeratedTypeQualifiedName { get; set; }
    }
}