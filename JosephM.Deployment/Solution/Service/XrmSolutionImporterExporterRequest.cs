#region

using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using System.Collections.Generic;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    [Group(Sections.Main, true, 10)]
    [Group(Sections.Connection, true, 20)]
    [Group(Sections.DataIncluded, true, 30)]
    [DisplayName("Solution Import/Export")]
    public class XrmSolutionImporterExporterRequest : ServiceRequestBase
    {
        [Group(Sections.Main)]
        [DisplayOrder(10)]
        [RequiredProperty]
        public SolutionImportExportTask? ImportExportTask { get; set; }

        [Group(Sections.Main)]
        [DisplayOrder(20)]
        [DisplayName("Folder To Export The Files Into")]
        [RequiredProperty]
        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { SolutionImportExportTask.ExportSolutions, SolutionImportExportTask.MigrateSolutions })]
        public Folder FolderPath { get; set; }

        [Group(Sections.Connection)]
        [DisplayOrder(100)]
        [DisplayName("Saved Connection For The CRM Instance To Import Into")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), "Connections")]
        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { SolutionImportExportTask.MigrateSolutions, SolutionImportExportTask.ImportSolutions })]
        public SavedXrmRecordConfiguration ImportToConnection { get; set; }

        [Group(Sections.DataIncluded)]
        [DisplayOrder(200)]
        [DisplayName("Folder To Include Exported Record XML Files In The Import (Optional)")]
        [PropertyInContextByPropertyValues("ImportExportTask", new object[] { SolutionImportExportTask.ImportSolutions })]
        public Folder IncludeImportDataInFolder { get; set; }

        [DisplayOrder(300)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", SolutionImportExportTask.ExportSolutions)]
        public IEnumerable<SolutionExport> SolutionExports { get; set; }

        [DisplayOrder(400)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", SolutionImportExportTask.ImportSolutions)]
        public IEnumerable<SolutionImport> SolutionImports { get; set; }

        [DisplayOrder(500)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("ImportExportTask", SolutionImportExportTask.MigrateSolutions)]
        public IEnumerable<SolutionMigration> SolutionMigrations { get; set; }

        private static class Sections
        {
            public const string Main = "Main";
            public const string Connection = "Connection";
            public const string DataIncluded = "Data Included In Import";
            public const string IncludeWithExportedRecords = "Options To Include With Exported Records";
        }
    }
}