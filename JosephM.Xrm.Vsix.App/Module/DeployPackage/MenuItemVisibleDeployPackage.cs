using JosephM.Application.Application;
using JosephM.Xrm.Vsix.Application;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.DeployPackage
{
    public class MenuItemVisibleDeployPackage : MenuItemVisible
    {
        public override bool IsVisible(IApplicationController applicationController)
        {
            var visualStudioService = applicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            var selectedItems = visualStudioService.GetSelectedItems();
            return selectedItems.Count() == 1
                && selectedItems.First() is ISolutionFolder
                && ((ISolutionFolder)selectedItems.First()).ParentProjectName == "Releases";
        }
    }
}
