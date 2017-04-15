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
    public class XrmSolutionImporterExporterRequest : ServiceRequestBase
    {
        [RequiredProperty]
        public SolutionImportExportTask? ImportExportTask { get; set; }

        [DisplayName("Folder To Export The Files Into")]
        [RequiredProperty]
        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { SolutionImportExportTask.ExportSolutions, SolutionImportExportTask.MigrateSolutions })]
        public Folder FolderPath { get; set; }

        [DisplayName("Saved Connection For The CRM Instance To Import Into")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { SolutionImportExportTask.MigrateSolutions, SolutionImportExportTask.ImportSolutions })]
        public SavedXrmRecordConfiguration ImportToConnection { get; set; }

        [DisplayName("Folder To Include Exported Record XML Files In The Import (Optional)")]
        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { SolutionImportExportTask.ImportSolutions })]
        public Folder IncludeImportDataInFolder { get; set; }

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