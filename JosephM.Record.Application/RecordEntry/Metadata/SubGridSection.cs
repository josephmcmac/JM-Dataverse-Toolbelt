#region

using System.Collections.Generic;

#endregion

namespace JosephM.Record.Application.RecordEntry.Metadata
{
    public class SubGridSection : FormSection
    {
        public SubGridSection(string sectionName, string linkedRecordType, string linkedRecordLookup,
            IEnumerable<GridFieldMetadata> fields)
            : base(sectionName)
        {
            Fields = fields;
            LinkedRecordLookup = linkedRecordLookup;
            LinkedRecordType = linkedRecordType;
        }

        public string LinkedRecordType { get; private set; }
        public string LinkedRecordLookup { get; private set; }
        public IEnumerable<GridFieldMetadata> Fields { get; private set; }
    }
}