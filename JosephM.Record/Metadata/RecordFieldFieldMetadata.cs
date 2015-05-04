#region

using System.Collections.Generic;
using JosephM.Core.FieldType;

#endregion

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class RecordFieldFieldMetadata : FieldMetadata
    {
        public RecordFieldFieldMetadata(string internalName, string label)
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
            get { return RecordFieldType.RecordField; }
        }
    }
}