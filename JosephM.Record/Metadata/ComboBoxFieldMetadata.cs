#region

using System.Collections.Generic;
using JosephM.Core.FieldType;

#endregion

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class ComboBoxFieldMetadata : FieldMetadata
    {
        private IEnumerable<PicklistOption> _items = new PicklistOption[] {};

        public ComboBoxFieldMetadata(string internalName, string label)
            : base(internalName, label)
        {
        }

        public ComboBoxFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
        }

        public IEnumerable<PicklistOption> Items
        {
            get { return _items; }
            set { _items = value; }
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Picklist; }
        }
    }
}