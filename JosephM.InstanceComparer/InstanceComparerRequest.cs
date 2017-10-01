using JosephM.Application.ViewModel.Attributes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using System.Collections.Generic;

namespace JosephM.InstanceComparer
{
    [Group(Sections.Connections, true, 10)]
    [Group(Sections.Folder, true, 20)]
    [DisplayName("Instance Comparison")]
    public class InstanceComparerRequest : ServiceRequestBase
    {
        [DisplayOrder(10)]
        [Group(Sections.Connections)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        [ConnectionFor("DataComparisons")]
        public SavedXrmRecordConfiguration ConnectionOne { get; set; }

        [DisplayOrder(20)]
        [Group(Sections.Connections)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        public SavedXrmRecordConfiguration ConnectionTwo { get; set; }

        [DisplayOrder(100)]
        [Group(Sections.Folder)]
        [RequiredProperty]
        public Folder Folder { get; set; }

        public IEnumerable<InstanceCompareDataCompare> DataComparisons { get; set; }

        [BulkAddRecordTypeFunction]
        public class InstanceCompareDataCompare
        {
            [Hidden]
            public string Type { get { return RecordType == null ? null : RecordType.Key; } }

            public RecordType RecordType { get; set; }

            public override string ToString()
            {
                return RecordType != null ? RecordType.Value : base.ToString();
            }
        }

        private static class Sections
        {
            public const string Folder = "Select The Folder To Save The Generated CSV Into";
            public const string Connections = "Select The Saved Connections For The CRM Instances To Compare";
        }
    }
}