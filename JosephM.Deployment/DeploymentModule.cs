using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.Grid;
using JosephM.Core.FieldType;
using JosephM.Deployment.CreatePackage;
using JosephM.Deployment.DeployPackage;
using JosephM.Deployment.DeploySolution;
using JosephM.Deployment.ExportXml;
using JosephM.Deployment.ImportCsvs;
using JosephM.Deployment.ImportExcel;
using JosephM.Deployment.ImportXml;
using JosephM.Deployment.MigrateRecords;
using JosephM.Record.Extentions;
using System;
using System.Linq;

namespace JosephM.Deployment
{
    [DependantModule(typeof(ImportXmlModule))]
    [DependantModule(typeof(ImportCsvsModule))]
    [DependantModule(typeof(ImportExcelModule))]
    [DependantModule(typeof(ExportXmlModule))]
    [DependantModule(typeof(CreatePackageModule))]
    [DependantModule(typeof(DeployPackageModule))]
    [DependantModule(typeof(MigrateRecordsModule))]
    [DependantModule(typeof(DeploySolutionModule))]
    public class DeploymentModule : ModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
            AddPortalDataButtonToExportRecordTypeGrid();
        }

        private void AddPortalDataButtonToExportRecordTypeGrid()
        {
            var customGridFunction = new CustomGridFunction("ADDPORTALDATA", "Add Portal Types", (DynamicGridViewModel g) =>
            {
                var r = g.ParentForm;
                if (r == null)
                    throw new NullReferenceException("Could Not Load The Form. The ParentForm Is Null");
                try
                {
                    var enumerableField = r.GetEnumerableFieldViewModel(g.ReferenceName);
                    var typesToAdd = new[]
                    {
                        "adx_contentsnippet",
                        "adx_entityform",
                        "adx_entityformmetadata",
                        "adx_entitylist",
                        "adx_entitypermission",
                        "adx_pagetemplate",
                        "adx_publishingstate",
                        "adx_sitemarker",
                        "adx_sitesetting",
                        "adx_webfile",
                        "adx_webform",
                        "adx_webformmetadata",
                        "adx_webformstep",
                        "adx_weblink",
                        "adx_weblinkset",
                        "adx_webpage",
                        "adx_webpageaccesscontrolrule",
                        "adx_webrole",
                        "adx_webtemplate",
                    };
                    foreach (var item in typesToAdd.Reverse())
                    {
                        var newRecord = g.RecordService.NewRecord(typeof(ExportRecordType).AssemblyQualifiedName);
                        newRecord.SetField(nameof(ExportRecordType.RecordType), new RecordType(item, item), g.RecordService);
                        enumerableField.InsertRecord(newRecord, 0);
                    }
                }
                catch (Exception ex)
                {
                    g.ApplicationController.ThrowException(ex);
                }
            }, visibleFunction: (g) =>
            {
                var lookupService = g.RecordService.GetLookupService(nameof(ExportRecordType.RecordType), typeof(ExportRecordType).AssemblyQualifiedName, g.ReferenceName, null);
                return lookupService != null && lookupService.RecordTypeExists("adx_webfile");
            });
            this.AddCustomGridFunction(customGridFunction, typeof(ExportRecordType));
        }
    }
}