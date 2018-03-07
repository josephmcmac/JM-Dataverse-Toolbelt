using JosephM.Application.ViewModel.Attributes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using System.Collections.Generic;

namespace JosephM.InstanceComparer
{
    [AllowSaveAndLoad]
    [Group(Sections.Connections, true, 10)]
    [Group(Sections.CompareOptions, true, order: 20, selectAll: true)]
    [DisplayName("Instance Comparison")]
    public class InstanceComparerRequest : ServiceRequestBase
    {
        [DisplayOrder(5)]
        [Group(Sections.Connections)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        [ConnectionFor("DataComparisons")]
        public SavedXrmRecordConfiguration ConnectionOne { get; set; }

        [DisplayOrder(10)]
        [Group(Sections.Connections)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        public SavedXrmRecordConfiguration ConnectionTwo { get; set; }

        [Group(Sections.CompareOptions)]
        [DisplayOrder(15)]
        public bool Solutions { get; set; }
        [Group(Sections.CompareOptions)]
        [DisplayOrder(20)]
        public bool Entities { get; set; }
        [Group(Sections.CompareOptions)]
        [DisplayOrder(30)]
        public bool Workflows { get; set; }
        [Group(Sections.CompareOptions)]
        [DisplayOrder(40)]
        public bool WebResources { get; set; }
        [Group(Sections.CompareOptions)]
        [DisplayOrder(50)]
        public bool Plugins { get; set; }
        [Group(Sections.CompareOptions)]
        [DisplayOrder(60)]
        public bool SharedOptions { get; set; }
        [Group(Sections.CompareOptions)]
        [DisplayOrder(70)]
        public bool SecurityRoles { get; set; }
        [Group(Sections.CompareOptions)]
        [DisplayOrder(80)]
        public bool Dashboards { get; set; }
        [Group(Sections.CompareOptions)]
        [DisplayOrder(85)]
        public bool EmailTemplates { get; set; }
        [Group(Sections.CompareOptions)]
        [DisplayOrder(87)]
        public bool Reports { get; set; }
        [Group(Sections.CompareOptions)]
        [DisplayOrder(88)]
        public bool CaseCreationRules { get; set; }

        [Group(Sections.CompareOptions)]
        [DisplayName("SLAs")]
        [DisplayOrder(89)]
        public bool SLAs { get; set; }

        [Group(Sections.CompareOptions)]
        [DisplayOrder(9000)]
        public bool Data { get; set; }

        [RequiredProperty]
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
            public const string CompareOptions = "CompareOptions";
            public const string Connections = "Select The Saved Connections For The CRM Instances To Compare";
        }
    }
}