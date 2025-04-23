using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.XrmModule.SavedXrmConnections;
using System.Collections.Generic;

namespace JosephM.InstanceComparer
{
    [DisplayName("Instance Comparison")]
    [Instruction("This process compares customisations across 2 instances and outputs a report of differences. Note only limited customisation types are compared")]
    [AllowSaveAndLoad]
    [Group(Sections.InstancesToCompare, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    [Group(Sections.CompareOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, selectAll: true)]
    [Group(Sections.GeneralOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 30, displayLabel: false)]
    [Group(Sections.TypesToInclude, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 40, displayLabel: false)]
    public class InstanceComparerRequest : ServiceRequestBase
    {
        public InstanceComparerRequest()
        {
            AllTypesForEntityMetadata = true;
            IgnoreMissingManagedComponentDifferences = true;
            IgnoreObjectTypeCodeDifferences = true;
        }

        [DisplayOrder(5)]
        [Group(Sections.InstancesToCompare)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections), allowAddNew: false)]
        [ConnectionFor(nameof(DataComparisons))]
        [ConnectionFor(nameof(EntityTypeComparisons))]
        public SavedXrmRecordConfiguration ConnectionOne { get; set; }

        [DisplayOrder(10)]
        [Group(Sections.InstancesToCompare)]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections), allowAddNew: false)]
        public SavedXrmRecordConfiguration ConnectionTwo { get; set; }

        [GridWidth(110)]
        [Group(Sections.GeneralOptions)]
        [DisplayOrder(14)]
        [MyDescription("When checked components in a managed solution which is not installed in either environment will be ignored")]
        public bool IgnoreMissingManagedComponentDifferences { get; set; }

        [GridWidth(120)]
        [Group(Sections.GeneralOptions)]
        [PropertyInContextByPropertyValue(nameof(Data), true)]
        [DisplayOrder(15)]
        [MyDescription("This option is to ignore primary key differences when comparing data")]
        public bool IgnorePrimaryKeyDifferencesInComparedData { get; set; }

        [GridWidth(120)]
        [Group(Sections.GeneralOptions)]
        [DisplayOrder(16)]
        [MyDescription("This option is to ignore differences in picklists of object type codes for shared and field picklists")]
        public bool IgnoreObjectTypeCodeDifferences { get; set; }

        [GridWidth(120)]
        [Group(Sections.GeneralOptions)]
        [PropertyInContextByPropertyValue(nameof(Solutions), true)]
        [DisplayOrder(17)]
        [MyDescription("Declares if hidden solutions should be compared")]
        public bool IncludeHiddenSolutions { get; set; }

        [GridWidth(120)]
        [Group(Sections.GeneralOptions)]
        [PropertyInContextByPropertyValue(nameof(Data), true)]
        [DisplayOrder(18)]
        [MyDescription("This option specifies if records should be matched across the instances by name instead of primary key")]
        public bool DataMatchByName { get; set; }

        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(17)]
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
        [DisplayOrder(75)]
        public bool FieldSecurityProfiles { get; set; }
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
        [DisplayOrder(120)]
        public bool OrganisationSettings { get; set; }

        [GridWidth(110)]
        [Group(Sections.CompareOptions)]
        [DisplayOrder(9000)]
        public bool Data { get; set; }

        [DisplayName("Entity Types for Data Comparison")]
        [GridWidth(400)]
        [RequiredProperty]
        [Group(Sections.TypesToInclude)]
        [PropertyInContextByPropertyValue(nameof(Data), true)]
        [PropertyInContextByPropertyNotNull(nameof(ConnectionOne))]
        public IEnumerable<InstanceCompareDataCompare> DataComparisons { get; set; }

        [GridWidth(390)]
        [Group(Sections.TypesToInclude)]
        [DisplayOrder(10)]
        [PropertyInContextByPropertyValue(nameof(Entities), true)]
        [DisplayName("All Entity Type Metadata")]
        public bool AllTypesForEntityMetadata { get; set; }

        [DisplayName("Entity Types for Metadata Comparison")]
        [Group(Sections.TypesToInclude)]
        [DisplayOrder(395)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(Entities), true)]
        [PropertyInContextByPropertyValue(nameof(AllTypesForEntityMetadata), false)]
        [PropertyInContextByPropertyNotNull(nameof(ConnectionOne))]
        public IEnumerable<InstanceCompareTypeCompare> EntityTypeComparisons { get; set; }

        [Group(Sections.Hidden, isHiddenSection: true)]
        [Group(Sections.GeneralOptions, Group.DisplayLayoutEnum.VerticalCentered)]
        [BulkAddRecordTypeFunction]
        public class InstanceCompareDataCompare
        {
            [Hidden]
            public string Type { get { return RecordType == null ? null : RecordType.Key; } }

            [Group(Sections.GeneralOptions)]
            [DisplayOrder(10)]
            [GridField]
            [GridWidth(400)]
            [RecordTypeFor(nameof(IncludeFields) + "." + nameof(FieldSetting.RecordField))]
            [RecordTypeFor(nameof(ExcludeFields) + "." + nameof(FieldSetting.RecordField))]
            public RecordType RecordType { get; set; }

            [Group(Sections.GeneralOptions)]
            [DisplayOrder(20)]
            [PropertyInContextByPropertyNotNull(nameof(RecordType))]
            [Multiline]
            public string FetchXmlFilter { get; set; }

            [Group(Sections.GeneralOptions)]
            [DisplayOrder(21)]
            [PropertyInContextByPropertyNotNull(nameof(FetchXmlFilter))]
            public bool UseAlternativeFilterForConnection2 { get; set; }

            [Group(Sections.GeneralOptions)]
            [DisplayOrder(22)]
            [PropertyInContextByPropertyNotNull(nameof(RecordType))]
            [PropertyInContextByPropertyValue(nameof(UseAlternativeFilterForConnection2), true)]
            [Multiline]
            public string Connection2FetchXmlFilter { get; set; }

            [Group(Sections.Hidden)]
            [DisplayOrder(20)]
            [PropertyInContextByPropertyNotNull(nameof(RecordType))]
            [GridField]
            [GridWidth(110)]
            public bool HasFilter { get { return !string.IsNullOrEmpty(FetchXmlFilter); } }

            [Group(Sections.GeneralOptions)]
            [DisplayOrder(25)]
            [PropertyInContextByPropertyNotNull(nameof(RecordType))]
            [GridField]
            [GridReadOnly]
            [GridWidth(100)]
            public bool SpecifyIncludeFields { get; set; }

            [Group(Sections.GeneralOptions)]
            [DisplayOrder(30)]
            [PropertyInContextByPropertyNotNull(nameof(RecordType))]
            [PropertyInContextByPropertyValue(nameof(SpecifyIncludeFields), true)]
            [RequiredProperty]
            public IEnumerable<FieldSetting> IncludeFields { get; set; }

            [Group(Sections.GeneralOptions)]
            [DisplayOrder(35)]
            [PropertyInContextByPropertyNotNull(nameof(RecordType))]
            [GridField]
            [GridReadOnly]
            [GridWidth(110)]
            public bool SpecifyExcludeFields { get; set; }

            [Group(Sections.GeneralOptions)]
            [DisplayOrder(40)]
            [PropertyInContextByPropertyNotNull(nameof(RecordType))]
            [PropertyInContextByPropertyValue(nameof(SpecifyExcludeFields), true)]
            [RequiredProperty]
            public IEnumerable<FieldSetting> ExcludeFields { get; set; }

            public override string ToString()
            {
                return RecordType != null ? RecordType.Value : base.ToString();
            }

            private static class Sections
            {
                public const string GeneralOptions = "General Options";
                public const string Hidden = "Hidden";
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
            public const string InstancesToCompare = "Instances to Compare";
            public const string CompareOptions = "Compare Options";
            public const string GeneralOptions = "Other Options";
            public const string TypesToInclude = "Types to Compare";
        }
    }
}