using System.Collections.Generic;
using JosephM.Core.FieldType;

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class RecordTypeFieldMetadata : FieldMetadata
    {
        public RecordTypeFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
        }

        private IEnumerable<RecordType> _items = new RecordType[] {};

        public IEnumerable<RecordType> Items
        {
            get { return _items; }
            set { _items = value; }
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.RecordType; }
        }
    }
}