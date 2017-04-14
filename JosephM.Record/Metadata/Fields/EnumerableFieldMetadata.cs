using System;

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class EnumerableFieldMetadata : FieldMetadata
    {
        public EnumerableFieldMetadata(string internalName, string label, Type enumeratedType)
            : base(internalName, label)
        {
            EnumeratedType = enumeratedType;
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Enumerable; }
        }

        public Type EnumeratedType { get; set; }

        public string EnumeratedTypeQualifiedName { get { return EnumeratedType.AssemblyQualifiedName; } }
    }
}