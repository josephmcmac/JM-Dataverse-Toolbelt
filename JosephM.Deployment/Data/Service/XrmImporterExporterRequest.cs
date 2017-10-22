#region

using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    [AllowSaveAndLoad]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.CsvImport, true, 20)]
    [Group(Sections.IncludeWithExportedRecords, true, 30)]
    [Group(Sections.Misc, true, 40)]
    [DisplayName("Data Import/Export")]
    public class XrmImporterExporterRequest : ServiceRequestBase
    {
        public XrmImporterExporterRequest()
        {
            MatchByName = true;
        }

        [DisplayOrder(10)]
        [Group(Sections.Main)]
        [RequiredProperty]
        public ImportExportTask? ImportExportTask { get; set; }

        [DisplayOrder(15)]
        [Group(Sections.Main)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ImportCsvs)]
        [InitialiseFor("ImportExportTask", Service.ImportExportTask.ExportXml, CsvImportOption.Folder, AlwaysSetIfNotEmpty =true)]
        [InitialiseFor("ImportExportTask", Service.ImportExportTask.ImportXml, CsvImportOption.Folder, AlwaysSetIfNotEmpty = true)]
        public CsvImportOption FolderOrFiles { get; set; }

        [DisplayOrder(20)]
        [Group(Sections.Main)]
        [RequiredProperty]
        [DisplayNameForPropertyValue("ImportExportTask", Service.ImportExportTask.ImportCsvs, "Select The Folder Containing The CSV Files")]
        [DisplayNameForPropertyValue("ImportExportTask", Service.ImportExportTask.ImportXml, "Select The Folder Containing The XML Files")]
        [DisplayNameForPropertyValue("ImportExportTask", Service.ImportExportTask.ExportXml, "Select The Folder To Export The XML Files Into")]
        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { Service.ImportExportTask.ImportCsvs, Service.ImportExportTask.ExportXml, Service.ImportExportTask.ImportXml })]
        [PropertyInContextByPropertyValue("FolderOrFiles", CsvImportOption.Folder)]
        public Folder Folder { get; set; }

        [Group(Sections.Main)]
        [DisplayOrder(30)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("FolderOrFiles", CsvImportOption.SpecificFiles)]
        public IEnumerable<CsvToImport> CsvsToImport { get; set; }

        [DisplayOrder(100)]
        [Group(Sections.CsvImport)]
        [DisplayName("Match Existing Records By Name When Importing")]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ImportCsvs)]
        public bool MatchByName { get; set; }

        [DisplayOrder(110)]
        [Group(Sections.CsvImport)]
        [DisplayName("Select The Format Of Any Dates In The CSV File")]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ImportCsvs)]
        public DateFormat DateFormat { get; set; }

        [Group(Sections.Misc)]
        [DisplayOrder(400)]
        [RequiredProperty]
        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { Service.ImportExportTask.ImportCsvs, Service.ImportExportTask.ImportXml })]
        public bool MaskEmails { get; set; }

        [DisplayOrder(200)]
        [Group(Sections.IncludeWithExportedRecords)]
        [DisplayName("Include Notes Attached To Exported Records")]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ExportXml)]
        public bool IncludeNotes { get; set; }

        [DisplayOrder(210)]
        [Group(Sections.IncludeWithExportedRecords)]
        [DisplayName("Include N:N Associations Between Exported Records")]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ExportXml)]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [DisplayOrder(300)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ExportXml)]
        public IEnumerable<ImportExportRecordType> RecordTypesToExport { get; set; }

        public enum CsvImportOption
        {
            Folder,
            SpecificFiles
        }

        public class CsvToImport
        {
            [RequiredProperty]
            [FileMask(FileMasks.CsvFile)]
            public FileReference Csv { get; set; }
        }

        private static class Sections
        {
            public const string Main = "Main";
            public const string CsvImport = "CSV Import Options";
            public const string Misc = "Misc";
            public const string IncludeWithExportedRecords = "Options To Include With Exported Records";
        }
    }
}