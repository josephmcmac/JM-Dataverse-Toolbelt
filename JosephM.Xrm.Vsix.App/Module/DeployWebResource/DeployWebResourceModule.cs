using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using JosephM.Application.Desktop.Module.ServiceRequest;

namespace JosephM.Xrm.Vsix.Module.DeployWebResource
{
    [DeployWebResourceMenuItemVisible]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class DeployWebResourceModule : ServiceRequestModule<DeployWebResourceDialog, DeployWebResourceService, DeployWebResourceRequest, DeployWebResourceResponse, DeployWebResourceResponseItem>
    {
        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");
            var files = visualStudioService.GetSelectedFileNamesQualified();
            visualStudioService.SaveSelectedFiles();

            var request = new DeployWebResourceRequest()
            {
                Files = files
            };
            var uri = new UriQuery();
            uri.AddObject(nameof(DeployWebResourceDialog.Request), request);
            uri.AddObject(nameof(DeployWebResourceDialog.SkipObjectEntry), true);
            ApplicationController.NavigateTo(typeof(DeployWebResourceDialog), uri);
        }
    }
}
