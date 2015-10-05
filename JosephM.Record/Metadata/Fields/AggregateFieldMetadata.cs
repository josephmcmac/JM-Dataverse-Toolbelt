#region

using System;
using System.Collections.Generic;
using JosephM.Core.FieldType;
using JosephM.Record.Query;

#endregion

namespace JosephM.Record.Metadata
{
    /// <summary>
    ///     Stored the metadata for a field
    /// </summary>
    public class AggregateFieldMetadata : FieldMetadata
    {
        public IEnumerable<Condition> Conditions { get; set; }
        public string LinkedLookup { get; set; }
        public string LinkedType { get; set; }

        public AggregateFieldMetadata(string internalName, string label, string linkedLookup, string linkedType, IEnumerable<Condition> conditions)
            : base(internalName, label)
        {
            Conditions = conditions ?? new Condition[0];
            LinkedType = linkedType;
            LinkedLookup = linkedLookup;
            MinValue = Int32.MinValue;
            MinValue = Int32.MaxValue;
        }

        public override bool IsPersistent { get { return false; } }

        public override RecordFieldType FieldType
        {
            get { return RecordFieldType.Integer; }
        }
    }
}