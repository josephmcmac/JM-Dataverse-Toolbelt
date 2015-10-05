#region

using System.Collections.Generic;
using JosephM.Core.FieldType;

#endregion

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class StateFieldMetadata : FieldMetadata
    {
        private PicklistOptionSet _picklistOptionSet = new PicklistOptionSet();

        public StateFieldMetadata(string recordType, string internalName, string label)
            : base(recordType, internalName, label)
        {
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.State; }
        }
    }
}