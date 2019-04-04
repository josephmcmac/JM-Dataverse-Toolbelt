using JosephM.Application.Application;
using JosephM.Core.AppConfig;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.AddPortalCode
{
    public class MenuItemVisibleForDeployIntoFieldProject : MenuItemVisible
    {
        public override bool IsVisible(IApplicationController applicationController)
        {
            var packageSettings = applicationController.ResolveType<XrmPackageSettings>();
            var visualStudioService = applicationController.ResolveType<IVisualStudioService>();
            if (visualStudioService == null)
                throw new NullReferenceException("visualStudioService");

            if (packageSettings.DeployIntoFieldProjects == null || !packageSettings.DeployIntoFieldProjects.Any())
                return true;

            var selectedItems = visualStudioService.GetSelectedItems();
            if (selectedItems.Count() == 1)
            {
                foreach (var selectedItem in selectedItems)
                {
                    if (packageSettings.DeployIntoFieldProjects.Any(p => p.ProjectName == selectedItem.Name))
                        return true;
                }
            }
            return false;
        }
    }
}
