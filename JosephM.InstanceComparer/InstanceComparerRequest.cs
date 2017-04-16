using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using System.Collections.Generic;

namespace JosephM.InstanceComparer
{
    [Group(Sections.Connections, true)]
    [Group(Sections.Folder, true)]
    [DisplayName("Instance Comparison")]
    public class InstanceComparerRequest : ServiceRequestBase
    {
        [Group(Sections.Connections)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        [ConnectionFor("DataComparisons")]
        public SavedXrmRecordConfiguration ConnectionOne { get; set; }

        [Group(Sections.Connections)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        public SavedXrmRecordConfiguration ConnectionTwo { get; set; }

        [Group(Sections.Folder)]
        [RequiredProperty]
        public Folder Folder { get; set; }

        public IEnumerable<InstanceCompareDataCompare> DataComparisons { get; set; }

        public class InstanceCompareDataCompare
        {
            [Hidden]
            public string Type { get { return RecordType == null ? null : RecordType.Key; } }

            public RecordType RecordType { get; set; }
        }

        private static class Sections
        {
            public const string Folder = "Select The Folder To Save The Generated CSV Into";
            public const string Connections = "Select The Saved Connections For The CRM Instances To Compare";
            public const string Solution = "Solution";
        }
    }
}