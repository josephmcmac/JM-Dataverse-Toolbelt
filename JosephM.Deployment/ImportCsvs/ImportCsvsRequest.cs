using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Sql;
using System.Collections.Generic;

namespace JosephM.Deployment.ImportCsvs
{
    [Instruction("All CSV Files Will Be Imported Into The Dynamics Instance. Matches To Update Records In The Target Will By Done By Either Primary Key, Then Name, Else If No Match Is Found A New Record Will Be Created")]
    [DisplayName("Import CSVs")]
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.CsvImport, true, 20)]
    [Group(Sections.Misc, true, 40)]
    public class ImportCsvsRequest : ServiceRequestBase, IValidatableObject
    {
        public ImportCsvsRequest()
        {
            MatchByName = true;
        }

        [Group(Sections.Main)]
        [DisplayOrder(30)]
        [RequiredProperty]
        public IEnumerable<CsvToImport> CsvsToImport { get; set; }

        [DisplayOrder(100)]
        [Group(Sections.CsvImport)]
        [DisplayName("Match Existing Records By Name When Importing")]
        [RequiredProperty]
        public bool MatchByName { get; set; }

        [Group(Sections.CsvImport)]
        [DisplayOrder(105)]
        [RequiredProperty]
        public bool UpdateOnly { get; set; }

        [DisplayOrder(110)]
        [Group(Sections.CsvImport)]
        [DisplayName("Select The Format Of Any Dates In The CSV File")]
        [RequiredProperty]
        public DateFormat DateFormat { get; set; }

        [Group(Sections.Misc)]
        [DisplayOrder(400)]
        [RequiredProperty]
        public bool MaskEmails { get; set; }

        public enum CsvImportOption
        {
            Folder,
            SpecificFiles
        }

        [DoNotAllowGridOpen]
        [Group(Sections.Main, true, 10)]
        public class CsvToImport : IMapSpreadsheetImport
        {
            [ConnectionFor(nameof(SourceType), typeof(CsvFileConnection))]
            [ConnectionFor(nameof(Mappings) + "." + nameof(CsvImportFieldMapping.SourceColumn), typeof(CsvFileConnection))]
            [RequiredProperty]
            [Group(Sections.Main)]
            [GridWidth(500)]
            [DisplayOrder(10)]
            [FileMask(FileMasks.CsvFile)]
            public FileReference SourceCsv { get; set; }

            [Hidden]
            [Group(Sections.Main)]
            [DisplayOrder(20)]
            [InitialiseIfOneOption]
            [RecordTypeFor(nameof(Mappings) + "." + nameof(CsvImportFieldMapping.SourceColumn))]
            [PropertyInContextByPropertyNotNull(nameof(SourceCsv))]
            public RecordType SourceType { get; set; }

            [Group(Sections.Main)]
            [DisplayOrder(30)]
            [RequiredProperty]
            [IncludeManyToManyIntersects]
            [RecordTypeFor(nameof(Mappings) + "." + nameof(CsvImportFieldMapping.TargetField))]
            [RecordTypeFor(nameof(AltMatchKeys) + "." + nameof(CsvImportMatchKey.TargetField))]
            [PropertyInContextByPropertyNotNull(nameof(SourceCsv))]
            public RecordType TargetType { get; set; }

            [AllowNestedGridEdit]
            [PropertyInContextByPropertyNotNull(nameof(TargetType))]
            public IEnumerable<CsvImportMatchKey> AltMatchKeys { get; set; }

            [AllowNestedGridEdit]
            [RequiredProperty]
            [GridWidth(800)]
            [PropertyInContextByPropertyNotNull(nameof(SourceType))]
            [PropertyInContextByPropertyNotNull(nameof(TargetType))]
            [PropertyInContextByPropertyNotNull(nameof(SourceCsv))]
            public IEnumerable<CsvImportFieldMapping> Mappings { get; set; }

            string IMapSpreadsheetImport.SourceType => SourceType?.Key;

            string IMapSpreadsheetImport.TargetType => TargetType?.Key;
            string IMapSpreadsheetImport.TargetTypeLabel => TargetType?.Value;

            IEnumerable<IMapSpreadsheetMatchKey> IMapSpreadsheetImport.AltMatchKeys => AltMatchKeys;

            IEnumerable<IMapSpreadsheetColumn> IMapSpreadsheetImport.FieldMappings => Mappings;

            public override string ToString()
            {
                return (SourceType?.Value ?? "(None)") + " > " + (TargetType?.Value ?? "(None)");
            }

            private static class Sections
            {
                public const string Main = "Main";
            }

            [DoNotAllowGridOpen]
            public class CsvImportFieldMapping : IMapSpreadsheetColumn
            {
                [RequiredProperty]
                public RecordField SourceColumn { get; set; }

                [RequiredProperty]
                public RecordField TargetField { get; set; }

                string IMapSpreadsheetColumn.SourceField => SourceColumn?.Key;

                string IMapSpreadsheetColumn.TargetField => TargetField?.Key;

                public override string ToString()
                {
                    return (SourceColumn?.Value ?? "(None)") + " > " + (TargetField?.Value ?? "(None)");
                }
            }

            [DoNotAllowGridOpen]
            public class CsvImportMatchKey : IMapSpreadsheetMatchKey
            {
                [RequiredProperty]
                public RecordField TargetField { get; set; }

                string IMapSpreadsheetMatchKey.TargetField => TargetField?.Key;
                string IMapSpreadsheetMatchKey.TargetFieldLabel => TargetField?.Value;

                public override string ToString()
                {
                    return (TargetField?.Value ?? "(Empty)");
                }
            }
        }

        private static class Sections
        {
            public const string Main = "Main";
            public const string CsvImport = "CSV Import Options";
            public const string Misc = "Misc";
        }

        public IsValidResponse Validate()
        {
            if (CsvsToImport == null)
                return new IsValidResponse();

            return CsvsToImport.Validate(MatchByName, UpdateOnly);
        }
    }
}