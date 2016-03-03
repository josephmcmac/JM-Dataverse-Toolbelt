#region

using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using System.Collections.Generic;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    [DisplayName("Solution Import/Export")]
    [AllowSaveAndLoad]
    public class XrmSolutionImporterExporterRequest : ServiceRequestBase
    {
        [RequiredProperty]
        public SolutionImportExportTask? ImportExportTask { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { SolutionImportExportTask.ExportSolutions, SolutionImportExportTask.MigrateSolutions })]
        public Folder FolderPath { get; set; }

        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { SolutionImportExportTask.ExportSolutions, SolutionImportExportTask.ImportSolutions })]
        public Folder IncludeImportDataInFolder { get; set; }

        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { SolutionImportExportTask.MigrateSolutions, SolutionImportExportTask.ImportSolutions })]
        public SavedXrmRecordConfiguration ImportToConnection { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", SolutionImportExportTask.ExportSolutions)]
        public IEnumerable<SolutionExport> SolutionExports { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", SolutionImportExportTask.ImportSolutions)]
        public IEnumerable<SolutionImport> SolutionImports { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", SolutionImportExportTask.MigrateSolutions)]
        public IEnumerable<SolutionMigration> SolutionMigrations { get; set; }

    }
}