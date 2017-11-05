using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.Deployment.DeployPackage;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Application;
using System;

namespace JosephM.Xrm.Vsix.Module.DeployPackage
{
    [MenuItemVisibleDeployPackage]
    [DependantModule(typeof(XrmConnectionModule))]
    public class VsixDeployPackageModule : DeployPackageModule
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
        }

        public override void DialogCommand()
        {
            string folder = null;

            var visualStudioService = ApplicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");

            var selectedItems = visualStudioService.GetSelectedItems();

            foreach (var selectedItem in selectedItems)
            {
                var solutionFolder = selectedItem as ISolutionFolder;
                if (solutionFolder != null)
                {
                    foreach (var item in solutionFolder.ProjectItems)
                    {
                        if (item.FileName?.EndsWith(".zip") ?? false)
                        {
                            folder = item.FileFolder;
                            break;
                        }
                    }
                }
            }
            if (folder == null)
                throw new Exception("Could not find the package directory. Could not find zip file in the selected solution folder");

            var request = DeployPackageRequest.CreateForDeployPackage(folder);

            var uri = new UriQuery();
            uri.AddObject(nameof(DeployPackageDialog.Request), request);
            ApplicationController.RequestNavigate("Main", typeof(DeployPackageDialog), uri);
        }
    }
}
