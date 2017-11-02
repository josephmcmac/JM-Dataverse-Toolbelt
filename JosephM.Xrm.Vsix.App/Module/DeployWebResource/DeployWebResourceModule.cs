using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.Xrm.Vsix.Utilities;
using System;

namespace JosephM.Xrm.Vsix.Module.DeployWebResource
{
    [DeployWebResourceMenuItemVisible]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class DeployWebResourceModule : ActionModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
        }

        public override void DialogCommand()
        {
            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");
            var files = visualStudioService.GetSelectedFileNamesQualified();

            var request = new DeployWebResourceRequest()
            {
                Files = files
            };
            var uri = new UriQuery();
            uri.AddObject(nameof(DeployWebResourceDialog.Request), request);
            uri.AddObject(nameof(DeployWebResourceDialog.SkipObjectEntry), true);
            ApplicationController.RequestNavigate("Main", typeof(DeployWebResourceDialog), uri);
        }
    }
}
