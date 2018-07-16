using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using JosephM.Application.Desktop.Module.ServiceRequest;

namespace JosephM.Xrm.Vsix.Module.DeployIntoField
{
    [DeployIntoFieldMenuItemVisible]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class DeployIntoFieldModule : ServiceRequestModule<DeployIntoFieldDialog, DeployIntoFieldService, DeployIntoFieldRequest, DeployIntoFieldResponse, DeployIntoFieldResponseItem>
    {
        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");
            var files = visualStudioService.GetSelectedFileNamesQualified();

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
