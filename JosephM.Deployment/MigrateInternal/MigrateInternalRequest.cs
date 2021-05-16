using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Attributes;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;
using System.Collections.Generic;

namespace JosephM.Deployment.MigrateInternal
{
    [DisplayName("Migrate Internal")]
    [AllowSaveAndLoad]
    [Group(Sections.Options, true, 10)]
    [Group(Sections.Main, displayLayout: Group.DisplayLayoutEnum.HorizontalWrap, order: 20, displayLabel: false)]
    public class MigrateInternalRequest : ServiceRequestBase
    {
        public MigrateInternalRequest()
        {
            ExecuteMultipleSetSize = 50;
            TargetCacheLimit = 1000;
        }

        [Group(Sections.Options)]
        [DisplayOrder(410)]
        [RequiredProperty]
        public bool MatchRecordsByName { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(420)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(1000)]
        public int? ExecuteMultipleSetSize { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(425)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(5000)]
        public int? TargetCacheLimit { get; set; }

        [FormEntry]
        [Group(Sections.Main)]
        [DisplayOrder(1000)]
        [AllowGridFullScreen]
        [RequiredProperty]
        public IEnumerable<MigrateInternalTypeMapping> TypesToMigrate { get; set; }

        [FormEntry]
        [Group(Sections.Main)]
        [DisplayOrder(1100)]
        [AllowGridFullScreen]
        public IEnumerable<ReferenceFieldsForCopy> ReferenceFieldReplacements { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
            public const string Options = "Options";
        }

        [DoNotAllowGridEdit]
        [DoNotAllowGridOpen]
        [Group(Sections.Main, true, 20)]
        public class ReferenceFieldsForCopy
        {
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

        [DoNotAllowGridEdit]
        [Group(Sections.Main, true, 10)]
        public class MigrateInternalTypeMapping : IMapSourceImport
        {
            [Group(Sections.Main)]
            [DisplayOrder(10)]
            [RequiredProperty]
            [IncludeManyToManyIntersects]
            [RecordTypeFor(nameof(FieldMappings) + "." + nameof(MigrateInternalFieldMapping.SourceField))]
            public RecordType SourceType { get; set; }

            [Group(Sections.Main)]
            [DisplayOrder(20)]
            [RequiredProperty]
            [IncludeManyToManyIntersects]
            [RecordTypeFor(nameof(FieldMappings) + "." + nameof(MigrateInternalFieldMapping.TargetField))]
            public RecordType TargetType { get; set; }

            [GridWidth(350)]
            [AllowNestedGridEdit]
            [RequiredProperty]
            [PropertyInContextByPropertyNotNull(nameof(SourceType))]
            [PropertyInContextByPropertyNotNull(nameof(TargetType))]
            public IEnumerable<MigrateInternalFieldMapping> FieldMappings { get; set; }

            string IMapSourceImport.SourceType => SourceType?.Key;
            string IMapSourceImport.TargetType => TargetType?.Key;
            string IMapSourceImport.TargetTypeLabel => TargetType?.Value;
            bool IMapSourceImport.IgnoreDuplicates => false;
            IEnumerable<IMapSourceMatchKey> IMapSourceImport.AltMatchKeys => null;
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

                string IMapSourceField.SourceField => SourceField?.Key;

                string IMapSourceField.TargetField => TargetField?.Key;

                bool IMapSourceField.UseAltMatchField => false;

                string IMapSourceField.AltMatchFieldType => null;

                string IMapSourceField.AltMatchField => null;

                public override string ToString()
                {
                    return (SourceField?.Value ?? "(None)") + " > " + (TargetField?.Value ?? "(None)");
                }
            }
        }
    }
}