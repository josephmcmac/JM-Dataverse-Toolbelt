using JosephM.Application;
using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;
using System;

namespace JosephM.Xrm.Vsix.Module.DeployIntoField
{
    [DeployIntoFieldMenuItemVisible]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class DeployIntoFieldModule : ServiceRequestModule<DeployIntoFieldDialog, DeployIntoFieldService, DeployIntoFieldRequest, DeployIntoFieldResponse, DeployIntoFieldResponseItem>
    {
        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");

            var files = visualStudioService.GetSelectedFileNamesQualified();
            visualStudioService.SaveSelectedFiles();

            var request = new DeployIntoFieldRequest()
            {
                Files = files
            };
            var uri = new UriQuery();
            uri.AddObject(nameof(DeployIntoFieldDialog.Request), request);
            uri.AddObject(nameof(DeployIntoFieldDialog.SkipObjectEntry), true);
            ApplicationController.NavigateTo(typeof(DeployIntoFieldDialog), uri);
        }
    }
}
