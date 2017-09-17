#region

using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Record.Attributes;
using JosephM.Xrm.Schema;
using System.Collections.Generic;
using System;
using JosephM.Core.Utility;
using System.Linq;
using JosephM.Core.Extentions;
using System.IO;

#endregion

namespace JosephM.Xrm.ImportExporter.Service
{
    [Group(Sections.Main, true, 10)]
    [Group(Sections.Connection, true, 20)]
    [Group(Sections.PackageSolution, true, 25)]
    [Group(Sections.DataIncluded, true, 30)]
    [DisplayName("Solution Deployment")]
    public class XrmSolutionImporterExporterRequest : ServiceRequestBase, IValidatableObject
    {
        public static XrmSolutionImporterExporterRequest CreateForDeployPackage(string folder)
        {
            return new XrmSolutionImporterExporterRequest()
            {
                ImportExportTask = SolutionImportExportTask.DeployPackage,
                FolderContainingPackage = new Folder(folder),
                HideTypeAndFolder = true
            };
        }

        public static XrmSolutionImporterExporterRequest CreateForCreatePackage(string folder, Lookup solution)
        {
            return new XrmSolutionImporterExporterRequest()
            {
                ImportExportTask = SolutionImportExportTask.CreateDeploymentPackage,
                FolderPath = new Folder(folder),
                Solution = solution,
                HideTypeAndFolder = true
            };
        }

        public IsValidResponse Validate()
        {
            var response = new IsValidResponse();
            if(ImportExportTask == SolutionImportExportTask.CreateDeploymentPackage
                && FolderPath != null
                && Directory.Exists(FolderPath.FolderPath))
            {
                if (FileUtility.GetFiles(FolderPath.FolderPath).Any(f => f.EndsWith("zip")))
                {
                    response.AddInvalidReason(string.Format("{0} Already Contains .ZIP files. Remove The .ZIP Files Or Select Another Folder", GetType().GetProperty(nameof(FolderPath)).GetDisplayName()));
                }
            }
            return response;
        }

        [Group(Sections.Main)]
        [DisplayOrder(10)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(HideTypeAndFolder), false)]
        public SolutionImportExportTask? ImportExportTask { get; set; }

        [Group(Sections.Main)]
        [DisplayOrder(20)]
        [DisplayName("Folder To Export The Files Into")]
        [RequiredProperty]
        [PropertyInContextByPropertyValues(nameof(ImportExportTask), new object[] { SolutionImportExportTask.ExportSolutions, SolutionImportExportTask.MigrateSolutions, SolutionImportExportTask.CreateDeploymentPackage })]
        [PropertyInContextByPropertyValue(nameof(HideTypeAndFolder), false)]
        public Folder FolderPath { get; set; }

        [Group(Sections.Main)]
        [DisplayOrder(40)]
        [DisplayName("Deploy Package Into (Optional)")]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        [PropertyInContextByPropertyValues(nameof(ImportExportTask), new object[] { SolutionImportExportTask.CreateDeploymentPackage })]
        [PropertyInContextByPropertyValue(nameof(HideTypeAndFolder), false)]
        public SavedXrmRecordConfiguration DeployPackageInto { get; set; }

        [Group(Sections.Connection)]
        [DisplayOrder(100)]
        [DisplayName("Saved Connection To Import Into")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        [PropertyInContextByPropertyValues(nameof(ImportExportTask), new object[] { SolutionImportExportTask.MigrateSolutions, SolutionImportExportTask.ImportSolutions, SolutionImportExportTask.DeployPackage })]
        public SavedXrmRecordConfiguration Connection { get; set; }


        [Group(Sections.Main)]
        [DisplayOrder(20)]
        [RequiredProperty]
        [PropertyInContextByPropertyValues(nameof(ImportExportTask), new object[] { SolutionImportExportTask.DeployPackage })]
        [PropertyInContextByPropertyValue(nameof(HideTypeAndFolder), false)]
        public Folder FolderContainingPackage { get; set; }

        [DisplayOrder(510)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(ImportExportTask), SolutionImportExportTask.CreateDeploymentPackage)]
        [Group(Sections.PackageSolution)]
        [DisplayName("Solution")]
        [GridWidth(400)]
        [ReferencedType(Entities.solution)]
        [LookupCondition(Fields.solution_.ismanaged, false)]
        [LookupCondition(Fields.solution_.isvisible, true)]
        [LookupFieldCascade(nameof(ThisReleaseVersion), Fields.solution_.version)]
        [UsePicklist(Fields.solution_.uniquename)]
        public Lookup Solution { get; set; }

        [Group(Sections.PackageSolution)]
        [PropertyInContextByPropertyValue(nameof(ImportExportTask), SolutionImportExportTask.CreateDeploymentPackage)]
        [DisplayOrder(500)]
        public bool ExportAsManaged { get; set; }

        [Group(Sections.PackageSolution)]
        [PropertyInContextByPropertyValue(nameof(ImportExportTask), SolutionImportExportTask.CreateDeploymentPackage)]
        [PropertyInContextByPropertyNotNull(nameof(Solution))]
        [CascadeOnChange(nameof(SetVersionPostRelease))]
        [DisplayOrder(520)]
        [RequiredProperty]
        public string ThisReleaseVersion { get; set; }

        [Group(Sections.PackageSolution)]
        [PropertyInContextByPropertyValue(nameof(ImportExportTask), SolutionImportExportTask.CreateDeploymentPackage)]
        [PropertyInContextByPropertyNotNull(nameof(Solution))]
        [DisplayOrder(530)]
        [RequiredProperty]
        public string SetVersionPostRelease { get; set; }

        [Group(Sections.DataIncluded)]
        [DisplayOrder(1050)]
        [PropertyInContextByPropertyValue(nameof(ImportExportTask), SolutionImportExportTask.CreateDeploymentPackage)]
        public bool IncludeNotes { get; set; }

        [Group(Sections.DataIncluded)]
        [DisplayOrder(1060)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(ImportExportTask), SolutionImportExportTask.CreateDeploymentPackage)]
        public bool IncludeNNRelationshipsBetweenEntities { get; set; }

        [DisplayOrder(1070)]
        [GridWidth(400)]
        [PropertyInContextByPropertyValue(nameof(ImportExportTask), SolutionImportExportTask.CreateDeploymentPackage)]
        public IEnumerable<ImportExportRecordType> DataToInclude { get; set; }

        [Hidden]
        public bool HideTypeAndFolder { get; set; }

        [Group(Sections.DataIncluded)]
        [DisplayOrder(200)]
        [DisplayName("Folder To Include Exported Record XML Files In The Import (Optional)")]
        [PropertyInContextByPropertyValues(nameof(ImportExportTask), new object[] { SolutionImportExportTask.ImportSolutions })]
        public Folder IncludeImportDataInFolder { get; set; }

        [DisplayOrder(300)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(ImportExportTask), SolutionImportExportTask.ExportSolutions)]
        public IEnumerable<SolutionExport> SolutionExports { get; set; }

        [DisplayOrder(400)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(ImportExportTask), SolutionImportExportTask.ImportSolutions)]
        public IEnumerable<SolutionImport> SolutionImports { get; set; }

        [DisplayOrder(500)]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(ImportExportTask), SolutionImportExportTask.MigrateSolutions)]
        public IEnumerable<SolutionMigration> SolutionMigrations { get; set; }


        private static class Sections
        {
            public const string Main = "Main";
            public const string PackageSolution = "PackageSolution";
            public const string Connection = "Connection";
            public const string DataIncluded = "Data Included Options";
        }

    }
}