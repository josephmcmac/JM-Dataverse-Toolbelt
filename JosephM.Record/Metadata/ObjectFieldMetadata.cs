#region

using System.Collections.Generic;
using JosephM.Core.FieldType;
using System;

#endregion

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class ObjectFieldMetadata : FieldMetadata
    {
        public ObjectFieldMetadata(string recordType, string internalName, string label, Type propertyOfType)
            : base(recordType, internalName, label)
        {
            PropertyOfType = propertyOfType;
        }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Object; }
        }

        public Type PropertyOfType { get; private set; }
    }
}