#region

using System.Collections.Generic;
using JosephM.Core.FieldType;

#endregion

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class AnyFieldMetadata : FieldMetadata
    {
        public AnyFieldMetadata(string recordType, string internalName, string label, RecordFieldType fieldType)
            : base(recordType, internalName, label)
        {
            _fieldType = fieldType;
        }

        private RecordFieldType _fieldType;

        public override RecordFieldType FieldType
        {
            get { return _fieldType; }
        }
    }
}