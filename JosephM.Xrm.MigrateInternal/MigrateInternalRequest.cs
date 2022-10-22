using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using JosephM.Xrm.DataImportExport.MappedImport;
using System.Collections.Generic;

namespace JosephM.Xrm.MigrateInternal
{
    [DisplayName("Migrate Internal")]
    [AllowSaveAndLoad]

    [Group(Sections.ImportOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20)]
    public class MigrateInternalRequest : ServiceRequestBase
    {
        public MigrateInternalRequest()
        {
            ExecuteMultipleSetSize = 50;
            TargetCacheLimit = 1000;
        }

        [Group(Sections.ImportOptions)]
        [DisplayOrder(410)]
        [RequiredProperty]
        public bool MatchRecordsByName { get; set; }

        [Group(Sections.ImportOptions)]
        [DisplayOrder(415)]
        [RequiredProperty]
        public bool RetainPrimaryKey { get; set; }

        [Group(Sections.ImportOptions)]
        [DisplayOrder(420)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(1000)]
        public int? ExecuteMultipleSetSize { get; set; }

        [Group(Sections.ImportOptions)]
        [DisplayOrder(425)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(5000)]
        public int? TargetCacheLimit { get; set; }

        [DisplayOrder(1000)]
        [AllowGridFullScreen]
        [RequiredProperty]
        public IEnumerable<MigrateInternalTypeMapping> TypesToMigrate { get; set; }

        [DisplayOrder(1100)]
        [AllowGridFullScreen]
        public IEnumerable<ReferenceFieldsForCopy> ReferenceFieldReplacements { get; set; }

        private static class Sections
        {
            public const string ImportOptions = "Import Options";
        }

        [DoNotAllowGridOpen]
        public class ReferenceFieldsForCopy
        {
            [GridWidth(300)]
            [DisplayOrder(10)]
            [RequiredProperty]
            [RecordTypeFor(nameof(OldField))]
            [RecordTypeFor(nameof(NewField))]
            public RecordType ReferencingType { get; set; }

            [DisplayOrder(20)]
            [RequiredProperty]
            [PropertyInContextByPropertyNotNull(nameof(ReferencingType))]
            [LookupCondition(nameof(IFieldMetadata.FieldType), ConditionType.In, new[] { RecordFieldType.Lookup })]
            public RecordField OldField { get; set; }

            [DisplayOrder(30)]
            [RequiredProperty]
            [PropertyInContextByPropertyNotNull(nameof(ReferencingType))]
            [LookupCondition(nameof(IFieldMetadata.FieldType), ConditionType.In, new[] { RecordFieldType.Lookup })]
            public RecordField NewField { get; set; }
        }

        [DoNotAllowGridOpen]
        [Group(Sections.Main, true, 10)]
        public class MigrateInternalTypeMapping : IMapSourceImport
        {
            [MyDescription("Type Of Query For Identifying Records To Be Included")]
            [GridWidth(140)]
            [DisplayOrder(0)]
            [Group(Sections.Main)]
            [RequiredProperty]
            public SourceDatasetType SourceDatasetType { get; set; }

            [Group(Sections.Main)]
            [GridWidth(200)]
            [DisplayOrder(10)]
            [RequiredProperty]
            [IncludeManyToManyIntersects]
            [RecordTypeFor(nameof(FieldMappings) + "." + nameof(MigrateInternalFieldMapping.SourceField))]
            [RecordTypeFor(nameof(SpecificRecordsToExport) + "." + nameof(LookupSetting.Record))]
            public RecordType SourceType { get; set; }

            [Group(Sections.Main)]
            [GridWidth(200)]
            [DisplayOrder(20)]
            [RequiredProperty]
            [IncludeManyToManyIntersects]
            [RecordTypeFor(nameof(FieldMappings) + "." + nameof(MigrateInternalFieldMapping.TargetField))]
            [RecordTypeFor(nameof(AltMatchKeys) + "." + nameof(MigrateInternalMatchKey.TargetField))]
            public RecordType TargetType { get; set; }

            [GridWidth(160)]
            [DisplayOrder(25)]
            [AllowNestedGridEdit]
            [PropertyInContextByPropertyNotNull(nameof(TargetType))]
            public IEnumerable<MigrateInternalMatchKey> AltMatchKeys { get; set; }

            [GridWidth(250)]
            [DisplayOrder(30)]
            [AllowNestedGridEdit]
            [RequiredProperty]
            [PropertyInContextByPropertyNotNull(nameof(SourceType))]
            [PropertyInContextByPropertyNotNull(nameof(TargetType))]
            public IEnumerable<MigrateInternalFieldMapping> FieldMappings { get; set; }

            [MyDescription("If Type = Specific Records This Defines The Records To Export")]
            [GridWidth(200)]
            [DisplayOrder(50)]
            [RequiredProperty]
            [PropertyInContextByPropertyValue(nameof(SourceDatasetType), SourceDatasetType.SpecificRecords)]
            [PropertyInContextByPropertyNotNull(nameof(SourceType))]
            public IEnumerable<LookupSetting> SpecificRecordsToExport { get; set; }

            [MyDescription("If Type = FetchXml This Defines The Fetch Xml Query To Use")]
            [DisplayOrder(100)]
            [GridWidth(250)]
            [DisplayName("Fetch XML")]
            [Multiline]
            [RequiredProperty]
            [PropertyInContextByPropertyValue(nameof(SourceDatasetType), SourceDatasetType.FetchXml)]
            public string FetchXml { get; set; }

            string IMapSourceImport.SourceType => SourceType?.Key;
            string IMapSourceImport.TargetType => TargetType?.Key;
            string IMapSourceImport.TargetTypeLabel => TargetType?.Value;
            bool IMapSourceImport.IgnoreDuplicates => false;
            IEnumerable<IMapSourceMatchKey> IMapSourceImport.AltMatchKeys => AltMatchKeys;
            IEnumerable<IMapSourceField> IMapSourceImport.FieldMappings => FieldMappings;

            public override string ToString()
            {
                return (SourceType?.Value ?? "(None)") + " > " + (TargetType?.Value ?? "(None)");
            }

            private static class Sections
            {
                public const string Main = "Main";
            }

            [DoNotAllowGridOpen]
            public class MigrateInternalFieldMapping : IMapSourceField
            {
                [RequiredProperty]
                public RecordField SourceField { get; set; }

                [RequiredProperty]
                public RecordField TargetField { get; set; }

                [RequiredProperty]
                [FieldInContextForPropertyTypes(nameof(TargetField), RecordFieldType.Lookup, RecordFieldType.Customer, RecordFieldType.Owner)]
                [PropertyInContextByPropertyNotNull(nameof(TargetField))]
                public bool UseAltMatchField { get; set; }

                [RequiredProperty]
                [PropertyInContextByPropertyValue(nameof(UseAltMatchField), true)]
                [RecordTypeFor(nameof(AltMatchField))]
                [InitialiseIfOneOption]
                [TargetTypesFor(nameof(TargetField))]
                public RecordType AltMatchFieldType { get; set; }

                [RequiredProperty]
                [PropertyInContextByPropertyNotNull(nameof(AltMatchFieldType))]
                [PropertyInContextByPropertyValue(nameof(UseAltMatchField), true)]
                public RecordField AltMatchField { get; set; }

                string IMapSourceField.SourceField => SourceField?.Key;

                string IMapSourceField.TargetField => TargetField?.Key;

                bool IMapSourceField.UseAltMatchField => UseAltMatchField;

                string IMapSourceField.AltMatchFieldType => AltMatchFieldType?.Key;

                string IMapSourceField.AltMatchField => AltMatchField?.Key;

                public override string ToString()
                {
                    return (SourceField?.Value ?? "(None)") + " > " + (TargetField?.Value ?? "(None)");
                }
            }

            [DoNotAllowGridOpen]
            public class MigrateInternalMatchKey : IMapSourceMatchKey
            {
                [RequiredProperty]
                public RecordField TargetField { get; set; }

                string IMapSourceMatchKey.TargetField => TargetField?.Key;
                string IMapSourceMatchKey.TargetFieldLabel => TargetField?.Value;
                bool IMapSourceMatchKey.CaseSensitive => false;

                public override string ToString()
                {
                    return (TargetField?.Value ?? "(Empty)");
                }
            }
        }
    }
}