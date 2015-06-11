#region

using System.Collections.Generic;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Application.SettingTypes;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    [AllowSaveAndLoad]
    public class XrmSolutionImporterExporterRequest : ServiceRequestBase
    {
        [RequiredProperty]
        public SolutionImportExportTask? ImportExportTask { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { SolutionImportExportTask.ExportSolutions, SolutionImportExportTask.MigrateSolutions })]
        public Folder FolderPath { get; set; }

        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.SolutionImportExportTask.MigrateSolutions)]
        public SavedXrmRecordConfiguration ImportToConnection { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.SolutionImportExportTask.ExportSolutions)]
        public IEnumerable<SolutionExport> SolutionExports { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.SolutionImportExportTask.ImportSolutions)]
        public IEnumerable<SolutionImport> SolutionImports { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", Service.SolutionImportExportTask.MigrateSolutions)]
        public IEnumerable<SolutionMigration> SolutionMigrations { get; set; }

    }
}