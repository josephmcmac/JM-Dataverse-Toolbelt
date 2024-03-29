﻿using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Csv;
using JosephM.Xrm.DataImportExport.MappedImport;
using System.Collections.Generic;

namespace JosephM.Xrm.CsvImport
{
    [Instruction("Add CSV files to the grid and map to the table and coilumn ti import into")]
    [DisplayName("Import CSVs")]
    [AllowSaveAndLoad]
    [Group(Sections.CsvFiles, true, 50)]
    [Group(Sections.Options, true, 20)]
    public class CsvImportRequest : ServiceRequestBase, IValidatableObject
    {
        public CsvImportRequest()
        {
            MatchByName = true;
            ExecuteMultipleSetSize = 50;
            TargetCacheLimit = 1000;
        }

        [AllowGridFullScreen]
        [Group(Sections.CsvFiles)]
        [DisplayOrder(30)]
        [RequiredProperty]
        public IEnumerable<CsvToImport> CsvsToImport { get; set; }

        [DisplayOrder(100)]
        [Group(Sections.Options)]
        [DisplayName("Match exiting rec ords by name whe n importing")]
        [RequiredProperty]
        public bool MatchByName { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(105)]
        [RequiredProperty]
        public bool UpdateOnly { get; set; }

        [DisplayOrder(110)]
        [Group(Sections.Options)]
        [DisplayName("Select the format of dates in the CSVs")]
        [RequiredProperty]
        public DateFormat DateFormat { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(400)]
        [RequiredProperty]
        public bool MaskEmails { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(410)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(1000)]
        public int? ExecuteMultipleSetSize { get; set; }

        [Group(Sections.Options)]
        [DisplayOrder(420)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(5000)]
        public int? TargetCacheLimit { get; set; }

        public enum CsvImportOption
        {
            Folder,
            SpecificFiles
        }

        [DoNotAllowGridOpen]
        [Group(Sections.Main, true, 10)]
        public class CsvToImport : IMapSourceImport
        {
            [Group(Sections.Main)]
            [DisplayOrder(5)]
            [RequiredProperty]
            [GridWidth(85)]
            public bool IgnoreDuplicates { get; set; }

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

            string IMapSourceImport.SourceType => SourceType?.Key;

            string IMapSourceImport.TargetType => TargetType?.Key;
            string IMapSourceImport.TargetTypeLabel => TargetType?.Value;

            IEnumerable<IMapSourceMatchKey> IMapSourceImport.AltMatchKeys => AltMatchKeys;

            IEnumerable<IMapSourceField> IMapSourceImport.FieldMappings => Mappings;

            public override string ToString()
            {
                return (SourceType?.Value ?? "(None)") + " > " + (TargetType?.Value ?? "(None)");
            }

            private static class Sections
            {
                public const string Main = "Main";
            }

            [DoNotAllowGridOpen]
            public class CsvImportFieldMapping : IMapSourceField
            {
                [RequiredProperty]
                public RecordField SourceColumn { get; set; }

                [RequiredProperty]
                public RecordField TargetField { get; set; }

                string IMapSourceField.SourceField => SourceColumn?.Key;

                string IMapSourceField.TargetField => TargetField?.Key;

                bool IMapSourceField.UseAltMatchField => false;

                string IMapSourceField.AltMatchFieldType => null;

                string IMapSourceField.AltMatchField => null;

                public override string ToString()
                {
                    return (SourceColumn?.Value ?? "(None)") + " > " + (TargetField?.Value ?? "(None)");
                }
            }

            [DoNotAllowGridOpen]
            public class CsvImportMatchKey : IMapSourceMatchKey
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

        private static class Sections
        {
            public const string CsvFiles = "Csv Files";
            public const string Options = "Options";
        }

        public IsValidResponse Validate()
        {
            if (CsvsToImport == null)
                return new IsValidResponse();

            return CsvsToImport.Validate(MatchByName, UpdateOnly);
        }
    }
}