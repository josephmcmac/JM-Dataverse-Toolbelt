using JosephM.Xrm.Vsix.Utilities;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.DeployPackage
{
    public class MenuItemVisibleDeployPackage : MenuItemVisible
    {
        public override bool IsVisible(IVisualStudioService visualStudioService)
        {
            var selectedItems = visualStudioService.GetSelectedItems();
            return selectedItems.Count() == 1
                && selectedItems.First() is ISolutionFolder
                && ((ISolutionFolder)selectedItems.First()).ParentProjectName == "Releases";
        }
    }
}
