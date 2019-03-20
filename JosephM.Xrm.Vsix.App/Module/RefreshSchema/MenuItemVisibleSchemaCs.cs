using JosephM.Application.Application;
using JosephM.Xrm.Vsix.Application;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.RefreshSchema
{
    public class MenuItemVisibleSchemaCs : MenuItemVisible
    {
        public override bool IsVisible(IApplicationController applicationController)
        {
            var visualStudioService = applicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            var selectedItems = visualStudioService.GetSelectedFileNamesQualified();
            return selectedItems.Count() == 1
                && selectedItems.First().ToLower().EndsWith("schema.cs");
        }
    }
}
