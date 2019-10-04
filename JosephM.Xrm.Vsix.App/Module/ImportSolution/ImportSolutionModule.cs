using JosephM.Application;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
using JosephM.Core.AppConfig;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;
using System;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.ImportSolution
{
    [MenuItemVisibleZip]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class ImportSolutionModule : ServiceRequestModule<ImportSolutionDialog, ImportSolutionService, ImportSolutionRequest, ImportSolutionResponse, ImportSolutionResponseItem>
    {
        public override void RegisterTypes()
        {
            base.RegisterTypes();

            AddDialogCompletionLinks();
        }

        private void AddDialogCompletionLinks()
        {
            this.AddCustomFormFunction(new CustomFormFunction("OPENINSTANCE"
                , (r) => $"Open {r.GetRecord().GetField(nameof(ImportSolutionResponse.Connection))}"
                , (r) =>
                {
                    try
                    {
                        var connection = r.GetRecord().GetField(nameof(ImportSolutionResponse.Connection)) as IXrmRecordConfiguration;
                        var serviceFactory = ApplicationController.ResolveType<IOrganizationConnectionFactory>();
                        ApplicationController.StartProcess(new XrmRecordService(connection, serviceFactory).WebUrl);
                    }
                    catch (Exception ex)
                    {
                        ApplicationController.ThrowException(ex);
                    }
                }
                , (r) => r.GetRecord().GetField(nameof(ImportSolutionResponse.Connection)) != null)
                , typeof(ImportSolutionResponse));
        }

        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");
            var selectedItems = visualStudioService.GetSelectedFileNamesQualified();
            if (selectedItems.Count() != 1)
            {
                ApplicationController.UserMessage("Only one file may be selected to import");
                return;
            }

            var request = new ImportSolutionRequest()
            {
                SolutionZip = new FileReference(selectedItems.First())
            };
            var uri = new UriQuery();
            uri.AddObject(nameof(ImportSolutionDialog.Request), request);
            ApplicationController.NavigateTo(typeof(ImportSolutionDialog), uri);
        }
    }
}
