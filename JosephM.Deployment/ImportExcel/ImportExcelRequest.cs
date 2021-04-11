using JosephM.Application.ViewModel.Attributes;
using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Attributes;
using JosephM.Record.Metadata;
using JosephM.Record.Sql;
using System.Collections.Generic;

namespace JosephM.Deployment.ImportExcel
{
    [DisplayName("Import Excel")]
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.Options, true, 20)]
    public class ImportExcelRequest : ServiceRequestBase, IValidatableObject
    {
        public ImportExcelRequest()
        {
            MatchRecordsByName = true;
            ExecuteMultipleSetSize = 50;
            TargetCacheLimit = 1000;
        }

        [DisplayOrder(15)]
        [Group(Sections.Main)]
        [RequiredProperty]
        [FileMask(FileMasks.ExcelFile)]
        [ConnectionFor(nameof(Mappings) + "." + nameof(ExcelImportTabMapping.SourceTab), typeof(ExcelFileConnection))]
        [ConnectionFor(nameof(Mappings) + "." + nameof(ExcelImportTabMapping.Mappings) + "." + nameof(ExcelImportTabMapping.ExcelImportFieldMapping.SourceColumn), typeof(ExcelFileConnection))]
        public FileReference ExcelFile { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(400)]
        [RequiredProperty]
        public bool MaskEmails { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(410)]
        [RequiredProperty]
        public bool MatchRecordsByName { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(415)]
        [RequiredProperty]
        public bool UpdateOnly { get; set; }

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

        [AllowGridFullScreen]
        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(ExcelFile))]
        public IEnumerable<ExcelImportTabMapping> Mappings { get; set; }

        public IsValidResponse Validate()
        {
            if (Mappings == null)
                return new IsValidResponse();

            return Mappings.Validate(MatchRecordsByName, UpdateOnly);
        }

        private static class Sections
        {
            public const string Main = "Main";
            public const string Options = "Options";
        }

        [DoNotAllowGridOpen]
        [Group(Sections.Main, true, 10)]
        public class ExcelImportTabMapping : IMapSpreadsheetImport
        {
            [Group(Sections.Main)]
            [DisplayOrder(5)]
            [RequiredProperty]
            [GridWidth(85)]
            public bool IgnoreDuplicates { get; set; }

            [Group(Sections.Main)]
            [DisplayOrder(10)]
            [RequiredProperty]
            [RecordTypeFor(nameof(Mappings) + "." + nameof(ExcelImportFieldMapping.SourceColumn))]
            public RecordType SourceTab { get; set; }

            [Group(Sections.Main)]
            [DisplayOrder(20)]
            [RequiredProperty]
            [IncludeManyToManyIntersects]
            [RecordTypeFor(nameof(Mappings) + "." + nameof(ExcelImportFieldMapping.TargetField))]
            [RecordTypeFor(nameof(AltMatchKeys) + "." + nameof(ExcelImportMatchKey.TargetField))]
            public RecordType TargetType { get; set; }

            [AllowNestedGridEdit]
            [PropertyInContextByPropertyNotNull(nameof(TargetType))]
            public IEnumerable<ExcelImportMatchKey> AltMatchKeys { get; set; }

            [AllowNestedGridEdit]
            [RequiredProperty]
            [GridWidth(800)]
            [PropertyInContextByPropertyNotNull(nameof(SourceTab))]
            [PropertyInContextByPropertyNotNull(nameof(TargetType))]
            public IEnumerable<ExcelImportFieldMapping> Mappings { get; set; }

            string IMapSpreadsheetImport.SourceType => SourceTab?.Key;
            string IMapSpreadsheetImport.TargetType => TargetType?.Key;
            string IMapSpreadsheetImport.TargetTypeLabel => TargetType?.Value;
            IEnumerable<IMapSpreadsheetMatchKey> IMapSpreadsheetImport.AltMatchKeys => AltMatchKeys;
            IEnumerable<IMapSpreadsheetColumn> IMapSpreadsheetImport.FieldMappings => Mappings;

            public override string ToString()
            {
                return (SourceTab?.Value ?? "(None)") + " > " + (TargetType?.Value ?? "(None)");
            }

            private static class Sections
            {
                public const string Main = "Main";
            }

            [DoNotAllowGridOpen]
            public class ExcelImportFieldMapping : IMapSpreadsheetColumn
            {
                [RequiredProperty]
                public RecordField SourceColumn { get; set; }

                [RequiredProperty]
                public RecordField TargetField { get; set; }

                [RequiredProperty]
                [FieldInContextForPropertyTypes(nameof(TargetField), RecordFieldType.Lookup, RecordFieldType.Customer, RecordFieldType.Owner, IncludeAssociationReferences = true)]
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

                string IMapSpreadsheetColumn.SourceField => SourceColumn?.Key;

                string IMapSpreadsheetColumn.TargetField => TargetField?.Key;

                bool IMapSpreadsheetColumn.UseAltMatchField => UseAltMatchField;

                string IMapSpreadsheetColumn.AltMatchFieldType => AltMatchFieldType?.Key;

                string IMapSpreadsheetColumn.AltMatchField => AltMatchField?.Key;

                public override string ToString()
                {
                    return (SourceColumn?.Value ?? "(None)") + " > " + (TargetField?.Value ?? "(None)") + (UseAltMatchField ? $"{AltMatchFieldType?.Key}.{AltMatchField?.Key}" : null);
                }
            }

            [DoNotAllowGridOpen]
            public class ExcelImportMatchKey : IMapSpreadsheetMatchKey
            {
                [RequiredProperty]
                public RecordField TargetField { get; set; }

                [RequiredProperty]
                public bool CaseSensitive { get; set; }

                string IMapSpreadsheetMatchKey.TargetField => TargetField?.Key;
                string IMapSpreadsheetMatchKey.TargetFieldLabel => TargetField?.Value;
                bool IMapSpreadsheetMatchKey.CaseSensitive => CaseSensitive;

                public override string ToString()
                {
                    return (TargetField?.Value ?? "(Empty)") + (CaseSensitive ? "{case sensitive)" : null);
                }
            }
        }
    }
}