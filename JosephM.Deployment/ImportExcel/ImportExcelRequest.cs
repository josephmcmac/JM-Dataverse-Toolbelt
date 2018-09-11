using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Deployment.SpreadsheetImport;
using JosephM.Record.Sql;
using System.Collections.Generic;

namespace JosephM.Deployment.ImportExcel
{
    [DisplayName("Import Excel")]
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.Misc, true, 20)]
    public class ImportExcelRequest : ServiceRequestBase
    {
        public ImportExcelRequest()
        {
            MatchRecordsByName = true;
        }

        [DisplayOrder(15)]
        [Group(Sections.Main)]
        [RequiredProperty]
        [FileMask(FileMasks.ExcelFile)]
        [ConnectionFor(nameof(Mappings) + "." + nameof(ExcelImportTabMapping.SourceTab), typeof(ExcelFileConnection))]
        [ConnectionFor(nameof(Mappings) + "." + nameof(ExcelImportTabMapping.Mappings) + "." + nameof(ExcelImportTabMapping.ExcelImportFieldMapping.SourceColumn), typeof(ExcelFileConnection))]
        public FileReference ExcelFile { get; set; }

        [Group(Sections.Misc)]
        [DisplayOrder(400)]
        [RequiredProperty]
        public bool MaskEmails { get; set; }

        [Group(Sections.Misc)]
        [DisplayOrder(410)]
        [RequiredProperty]
        public bool MatchRecordsByName { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyNotNull(nameof(ExcelFile))]
        public IEnumerable<ExcelImportTabMapping> Mappings { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
            public const string Misc = "Misc";
        }

        [DoNotAllowGridOpen]
        [Group(Sections.Main, true, 10)]
        public class ExcelImportTabMapping : IMapSpreadsheetImport
        {
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
            public RecordType TargetType { get; set; }

            [AllowNestedGridEdit]
            [RequiredProperty]
            [GridWidth(800)]
            [PropertyInContextByPropertyNotNull(nameof(SourceTab))]
            [PropertyInContextByPropertyNotNull(nameof(TargetType))]
            public IEnumerable<ExcelImportFieldMapping> Mappings { get; set; }

            string IMapSpreadsheetImport.SourceType => SourceTab?.Key;

            string IMapSpreadsheetImport.TargetType => TargetType?.Key;

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

                string IMapSpreadsheetColumn.SourceField => SourceColumn?.Key;

                string IMapSpreadsheetColumn.TargetField => TargetField?.Key;

                public override string ToString()
                {
                    return (SourceColumn?.Value ?? "(None)") + " > " + (TargetField?.Value ?? "(None)");
                }
            }
        }
    }
}