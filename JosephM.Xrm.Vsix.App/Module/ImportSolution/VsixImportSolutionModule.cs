using JosephM.Application;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Deployment.ImportSolution;
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
    public class VsixImportSolutionModule : ImportSolutionModule
    {
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

            var request = ImportSolutionRequest.CreateForImportSolution(selectedItems.First());

            var uri = new UriQuery();
            uri.AddObject(nameof(ImportSolutionDialog.Request), request);
            ApplicationController.NavigateTo(typeof(ImportSolutionDialog), uri);
        }
    }
}
