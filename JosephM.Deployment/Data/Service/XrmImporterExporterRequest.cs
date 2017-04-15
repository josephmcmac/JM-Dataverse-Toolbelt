#region

using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    [Group(Sections.CsvImport, true)]
    [Group(Sections.IncludeWithExportedRecords, true)]
    [DisplayName("Data Import/Export")]
    public class XrmImporterExporterRequest : ServiceRequestBase
    {
        public XrmImporterExporterRequest()
        {
            MatchByName = true;
        }

        [RequiredProperty]
        public ImportExportTask? ImportExportTask { get; set; }

        [RequiredProperty]
        [DisplayNameForPropertyValue("ImportExportTask", Service.ImportExportTask.ImportCsvs, "Select The Folder Containing The CSV Files")]
        [DisplayNameForPropertyValue("ImportExportTask", Service.ImportExportTask.ImportXml, "Select The Folder Containing The XML Files")]
        [DisplayNameForPropertyValue("ImportExportTask", Service.ImportExportTask.ExportXml, "Select The Folder To Export The XML Files Into")]
        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { Service.ImportExportTask.ImportCsvs, Service.ImportExportTask.ExportXml, Service.ImportExportTask.ImportXml })]
        public Folder Folder { get; set; }

        [Group(Sections.CsvImport)]
        [DisplayName("Match Existing Records By Name When Importing")]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ImportCsvs)]
        public bool MatchByName { get; set; }

        [Group(Sections.CsvImport)]
        [DisplayName("Select The Format Of Any Dates In The CSV File")]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ImportCsvs)]
        public DateFormat DateFormat { get; set; }

        [Group(Sections.IncludeWithExportedRecords)]
        [DisplayName("Include Notes Attached To Exported Records")]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ExportXml)]
        public bool IncludeNotes { get; set; }

        [Group(Sections.IncludeWithExportedRecords)]
        [DisplayName("Include N:N Associations Between Exported Records")]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ExportXml)]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ExportXml)]
        public IEnumerable<ImportExportRecordType> RecordTypesToExport { get; set; }

        private static class Sections
        {
            public const string CsvImport = "CSV Import Options";
            public const string IncludeWithExportedRecords = "Options To Include With Exported Records";
        }
    }
}