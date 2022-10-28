using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Deployment.SolutionsImport;
using JosephM.Record.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.DataImportExport.XmlExport;
using JosephM.XrmModule.SavedXrmConnections;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Deployment.DeployPackage
{
    [MyDescription("Import a solution package into an instance")]
    public class DeployPackageModule
        : ServiceRequestModule<DeployPackageDialog, DeployPackageService, DeployPackageRequest, DeployPackageResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Solution Deployment";

        public override void RegisterTypes()
        {
            base.RegisterTypes();

            this.AddSolutionDetailsFormEvent(typeof(DeployPackageSolutionImportItem), nameof(DeployPackageSolutionImportItem.SolutionZip), nameof(DeployPackageSolutionImportItem.IsManaged), nameof(DeployPackageSolutionImportItem.Version), getSourceSolutionMetadata: (revm) =>
            {
                var solutionFileReference = revm.GetRecord().GetField(nameof(DeployPackageSolutionImportItem.SolutionZip)) as FileReference;
                return solutionFileReference == null
                ? null
                : SolutionZipUtility.LoadSolutionZipMetadata(solutionFileReference.FileName);
            });

            AddSolutionImportItemTriggers();

            AddDialogCompletionLinks();
        }

        private void AddSolutionImportItemTriggers()
        {
            Action<RecordEntryViewModelBase> loadToImportItems = (revm) =>
            {
                var connection = revm.GetRecord().GetField(nameof(DeployPackageRequest.Connection)) as SavedXrmRecordConfiguration;
                var packageFolder = revm.GetFieldViewModel<FolderFieldViewModel>(nameof(DeployPackageRequest.FolderContainingPackage)).Value;

                var solutionFiles = packageFolder?.FolderPath != null && connection != null
                ? Directory.GetFiles(packageFolder.FolderPath, "*.zip")
                    .OrderBy(s => s)
                    .ToArray()
                    : new string[0];

                if (solutionFiles.Count() != (revm.GetFieldViewModel<EnumerableFieldViewModel>(nameof(DeployPackageRequest.SolutionsForDeployment))?.GridRecords?.Count ?? 0))
                {
                    revm.GetRecord().SetField(nameof(DeployPackageRequest.SolutionsForDeployment), solutionFiles
                            .Select(sf => new DeployPackageSolutionImportItem
                            {
                                SolutionZip = new FileReference(sf),
                                TargetConnection = connection
                            })
                            .ToArray(), revm.RecordService);
                    revm.GetFieldViewModel<EnumerableFieldViewModel>(nameof(DeployPackageRequest.SolutionsForDeployment)).DynamicGridViewModel.ReloadGrid();
                }
            };
            Action<RecordEntryViewModelBase> loadRecordsForImport = (revm) =>
            {
                var connection = revm.GetRecord().GetField(nameof(DeployPackageRequest.Connection)) as SavedXrmRecordConfiguration;
                var packageFolder = revm.GetFieldViewModel<FolderFieldViewModel>(nameof(DeployPackageRequest.FolderContainingPackage)).Value;

                var entities = new List<Entity>();
                if (packageFolder?.FolderPath != null)
                {
                    foreach (var childFolder in Directory.GetDirectories(packageFolder.FolderPath))
                    {
                        if (new DirectoryInfo(childFolder).Name == "Data")
                        {
                            entities = ImportXmlService.LoadEntitiesFromXmlFiles(childFolder, new LogController())
                            .Select(kv => kv.Value)
                            .ToList();
                        }
                    }
                }
                var recordsForImport = entities
                    .GroupBy(e => e.LogicalName)
                    .Select(kv => new DeployPackageRecordTypeImport
                    {
                        RecordType = kv.Key,
                        RecordCount = kv.Count()
                    })
                    .OrderByDescending(i => i.RecordType)
                    .ToArray();

                var enumerableViewModel = revm.GetFieldViewModel<EnumerableFieldViewModel>(nameof(DeployPackageRequest.RecordsForImport));
                if (recordsForImport.Count() != (enumerableViewModel?.GridRecords?.Count ?? 0))
                {
                    //enumerableViewModel.GridRecords.Clear();
                    //foreach (var recordForImport in recordsForImport)
                    //{
                    //    enumerableViewModel.InsertRecord(new ObjectRecord(recordForImport), 0);
                    //}
                    revm.GetRecord().SetField(nameof(DeployPackageRequest.RecordsForImport), recordsForImport, revm.RecordService);
                    revm.GetFieldViewModel<EnumerableFieldViewModel>(nameof(DeployPackageRequest.RecordsForImport)).DynamicGridViewModel.ReloadGrid();
                }
            };
            var changeFunction = new OnChangeFunction((RecordEntryViewModelBase revm, string changedField) =>
            {
                switch (changedField)
                {
                    case nameof(DeployPackageRequest.FolderContainingPackage):
                        {
                            loadRecordsForImport(revm);
                            loadToImportItems(revm);
                            break;
                        }
                    case nameof(DeployPackageRequest.Connection):
                        {
                            loadToImportItems(revm);
                            break;
                        }
                }
            });
            this.AddOnChangeFunction(changeFunction, typeof(DeployPackageRequest));
            var formLoadedFunction = new FormLoadedFunction((RecordEntryViewModelBase revm) =>
            {
                loadRecordsForImport(revm);
                loadToImportItems(revm);
            });
            this.AddFormLoadedFunction(formLoadedFunction, typeof(DeployPackageRequest));
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENINSTANCE"
                , (r) => $"Open {r.GetRecord().GetField(nameof(DeployPackageResponse.Connection))}"
                , (r) =>
                {
                    try
                    {
                        var connection = r.GetRecord().GetField(nameof(DeployPackageResponse.Connection)) as IXrmRecordConfiguration;
                        var serviceFactory = ApplicationController.ResolveType<IOrganizationConnectionFactory>();
                        ApplicationController.StartProcess(new XrmRecordService(connection, serviceFactory).WebUrl);
                    }
                    catch (Exception ex)
                    {
                        ApplicationController.ThrowException(ex);
                    }
                }
                , (r) => r.GetRecord().GetField(nameof(DeployPackageResponse.Connection)) != null)
                , typeof(DeployPackageResponse));
        }
    }
}