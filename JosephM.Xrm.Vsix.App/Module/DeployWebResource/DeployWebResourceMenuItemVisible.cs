using JosephM.Application.Application;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System.Collections.Generic;
using JosephM.Core.AppConfig;
using System;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.DeployWebResource
{
    public class DeployWebResourceMenuItemVisible : MenuItemVisibleForFileTypes
    {
        public override IEnumerable<string> ValidExtentions => DeployWebResourceService.WebResourceTypes.Keys;

        public override bool IsVisible(IApplicationController applicationController)
        {
            var packageSettings = applicationController.ResolveType<XrmPackageSettings>();
            var visualStudioService = applicationController.ResolveType<IVisualStudioService>();
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");

            if (packageSettings.WebResourceProjects == null || !packageSettings.WebResourceProjects.Any())
                return base.IsVisible(applicationController);

            var selectedItems = visualStudioService.GetSelectedItems();
            return selectedItems.All(si => packageSettings.WebResourceProjects.Any(w => w.ProjectName == si.NameOfContainingProject))
                && base.IsVisible(applicationController);
        }
    }
}