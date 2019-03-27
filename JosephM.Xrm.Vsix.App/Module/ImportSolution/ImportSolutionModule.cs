using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.FieldType;
using JosephM.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Linq;
using JosephM.Application.ViewModel.Extentions;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Xrm.Vsix.Module.ImportSolution
{
    [MenuItemVisibleZip]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
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
                        ApplicationController.StartProcess(new XrmRecordService(r.GetRecord().GetField(nameof(ImportSolutionResponse.Connection)) as IXrmRecordConfiguration).WebUrl);
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
