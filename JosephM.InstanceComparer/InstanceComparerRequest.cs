using JosephM.Application.ViewModel.Attributes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using System.Collections.Generic;

namespace JosephM.InstanceComparer
{
    [Group(Sections.Connections, true, 10)]
    [Group(Sections.Folder, true, 15)]
    [Group(Sections.CompareOptions, true, 20)]
    [DisplayName("Instance Comparison")]
    public class InstanceComparerRequest : ServiceRequestBase
    {
        [Group(Sections.CompareOptions)]
        public bool Solutions { get; set; }
        [Group(Sections.CompareOptions)]
        public bool Workflows { get; set; }
        [Group(Sections.CompareOptions)]
        public bool WebResource { get; set; }
        [Group(Sections.CompareOptions)]
        public bool Entities { get; set; }
        [Group(Sections.CompareOptions)]
        public bool Plugins { get; set; }
        [Group(Sections.CompareOptions)]
        public bool SharedOptions { get; set; }
        [Group(Sections.CompareOptions)]
        public bool SecurityRoles { get; set; }
        [Group(Sections.CompareOptions)]
        public bool CaseCreationRules { get; set; }
        [Group(Sections.CompareOptions)]
        public bool Data { get; set; }

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

        [PropertyInContextByPropertyValue(nameof(Data), true)]
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
            public const string CompareOptions = "CompareOptions";
            public const string Connections = "Select The Saved Connections For The CRM Instances To Compare";
        }
    }
}