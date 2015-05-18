#region

using System.Collections.Generic;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Application.SettingTypes;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    //todo script verify save and load
    [AllowSaveAndLoad]
    public class XrmImporterExporterRequest : ServiceRequestBase
    {
        [RequiredProperty]
        public ImportExportTask? ImportExportTask { get; set; }

        [RequiredProperty]
        public Folder FolderPath { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ImportCsvs)]
        public bool MatchByName { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ImportCsvs)]
        public DateFormat DateFormat { get; set; }

        //[RequiredProperty]
        //[InitialiseFor("ImportExportTask", Service.ImportExportTask.ImportCsvs, true)]
        //[PropertyInContextByPropertyValue("AllRecordTypes", false)]
        //public bool AllRecordTypes { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.ImportExportTask.ExportXml)]
        public IEnumerable<RecordTypeSetting> RecordTypes { get; set; }
    }
}