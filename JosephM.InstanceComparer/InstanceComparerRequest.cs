using JosephM.Application.ViewModel.Attributes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.XrmModule.SavedXrmConnections;
using System.Collections.Generic;

namespace JosephM.InstanceComparer
{
    [DisplayName("Instance Comparison")]
    [Instruction("The System Will Query And Compare The Components Across The 2 Dynamics Instances. After Completion A Detailed Report Will Output Of Differences Identified. The Report Will Include Hyperlinks To Open Items, As Well As A Feature To Add Some, Or All, Of The Components Into A Solution For Correcting The Different Components \n\nNote The Comparison Does Not Include Every Property Of The Components")]
    [AllowSaveAndLoad]
    [Group(Sections.Connections, true, 10)]
    [Group(Sections.CompareOptions, true, order: 20, selectAll: true)]
    [Group(Sections.GeneralOptions, true, order: 25)]
    [Group(Sections.DataComparisonOptions, true, order: 30)]
    [Group(Sections.EntityMetadataComparisonOptions, true, order: 40)]
    public class InstanceComparerRequest : ServiceRequestBase
    {
        public InstanceComparerRequest()
        {
            AllTypesForEntityMetadata = true;
            IgnoreMissingManagedComponentDifferences = true;
        }

        [DisplayOrder(5)]
        [Group(Sections.Connections)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        [ConnectionFor(nameof(DataComparisons))]
        [ConnectionFor(nameof(EntityTypeComparisons))]
        public SavedXrmRecordConfiguration ConnectionOne { get; set; }

        [DisplayOrder(10)]
        [Group(Sections.Connections)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        public SavedXrmRecordConfiguration ConnectionTwo { get; set; }

        [GridWidth(110)]
        [Group(Sections.GeneralOptions)]
        [DisplayOrder(14)]
        [MyDescription("When checked components in a managed solution which is not installed in either environment will be ignored")]
        public bool IgnoreMissingManagedComponentDifferences { get; set; }
        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(15)]
        public bool Solutions { get; set; }
        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(20)]
        public bool Entities { get; set; }
        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(30)]
        public bool Workflows { get; set; }
        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(40)]
        public bool WebResources { get; set; }
        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(50)]
        public bool Plugins { get; set; }
        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(60)]
        public bool SharedOptions { get; set; }
        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(70)]
        public bool SecurityRoles { get; set; }
        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(80)]
        public bool Dashboards { get; set; }
        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(85)]
        public bool EmailTemplates { get; set; }
        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(87)]
        public bool Reports { get; set; }
        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(88)]
        public bool CaseCreationRules { get; set; }
        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayName("SLAs")]
        [DisplayOrder(89)]
        public bool SLAs { get; set; }
        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(95)]
        public bool Apps { get; set; }

        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(100)]
        public bool RoutingRules { get; set; }

        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(9000)]
        public bool Data { get; set; }

        [GridWidth(400)]
        [RequiredProperty]
        [Group(Sections.DataComparisonOptions)]
        [PropertyInContextByPropertyValue(nameof(Data), true)]
        [PropertyInContextByPropertyNotNull(nameof(ConnectionOne))]
        public IEnumerable<InstanceCompareDataCompare> DataComparisons { get; set; }

        [GridWidth(110)]
        [Group(Sections.EntityMetadataComparisonOptions)]
        [DisplayOrder(10)]
        [PropertyInContextByPropertyValue(nameof(Entities), true)]
        [DisplayName("All Types")]
        public bool AllTypesForEntityMetadata { get; set; }

        [Group(Sections.EntityMetadataComparisonOptions)]
        [DisplayOrder(20)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(Entities), true)]
        [PropertyInContextByPropertyValue(nameof(AllTypesForEntityMetadata), false)]
        [PropertyInContextByPropertyNotNull(nameof(ConnectionOne))]
        public IEnumerable<InstanceCompareTypeCompare> EntityTypeComparisons { get; set; }

        [DoNotAllowGridOpen]
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

        [DoNotAllowGridOpen]
        [BulkAddRecordTypeFunction]
        public class InstanceCompareTypeCompare
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
            public const string Connections = "Select The Saved Connections For The CRM Instances To Compare";
            public const string CompareOptions = "Compare Options";
            public const string GeneralOptions = "General Options";
            public const string DataComparisonOptions = "Data Comparison Options";
            public const string EntityMetadataComparisonOptions = "Entity Metadata Comparison Options";
        }
    }
}