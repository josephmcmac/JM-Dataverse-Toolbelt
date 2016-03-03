#region

using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System.Collections.Generic;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    [DisplayName("Data Import/Export")]
    [AllowSaveAndLoad]
    public class XrmImporterExporterRequest : ServiceRequestBase
    {
        [RequiredProperty]
        public ImportExportTask? ImportExportTask { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { Service.ImportExportTask.ImportCsvs, Service.ImportExportTask.ExportXml, Service.ImportExportTask.ImportXml })]
        public Folder FolderPath { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ImportCsvs)]
        public bool MatchByName { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ImportCsvs)]
        public DateFormat DateFormat { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ExportXml)]
        public bool IncludeNotes { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ExportXml)]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ExportXml)]
        public IEnumerable<ImportExportRecordType> RecordTypes { get; set; }
    }
}