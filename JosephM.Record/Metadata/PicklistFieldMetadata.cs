#region

using System.Collections.Generic;
using JosephM.Core.FieldType;

#endregion

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class PicklistFieldMetadata : FieldMetadata
    {
        private PicklistOptionSet _picklistOptionSet = new PicklistOptionSet();

        public PicklistFieldMetadata(string recordType, string internalName, string label,
            PicklistOptionSet picklistOptions)
            : base(recordType, internalName, label)
        {
            if (picklistOptions != null)
                PicklistOptionSet = picklistOptions;
        }

        public PicklistFieldMetadata(string recordType, string internalName, string label,
            IEnumerable<PicklistOption> picklistOptions)
            : base(recordType, internalName, label)
        {
            if (picklistOptions != null)
                PicklistOptionSet = new PicklistOptionSet(picklistOptions);
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Picklist; }
        }

        public IEnumerable<PicklistOption> PicklistOptions
        {
            get { return PicklistOptionSet.PicklistOptions; }
        }

        public PicklistOptionSet PicklistOptionSet
        {
            get { return _picklistOptionSet; }
            private set { _picklistOptionSet = value; }
        }
    }
}