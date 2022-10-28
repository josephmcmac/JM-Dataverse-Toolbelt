using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Deployment.SolutionsImport;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.XrmModule.SavedXrmConnections;
using System;
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
                    var solutionImportItemGrid = revm.GetFieldViewModel<EnumerableFieldViewModel>(nameof(DeployPackageRequest.SolutionsForDeployment)).GridRecords;
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
            var changeFunction = new OnChangeFunction((RecordEntryViewModelBase revm, string changedField) =>
            {
                switch (changedField)
                {
                    case nameof(DeployPackageRequest.FolderContainingPackage):
                        {
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