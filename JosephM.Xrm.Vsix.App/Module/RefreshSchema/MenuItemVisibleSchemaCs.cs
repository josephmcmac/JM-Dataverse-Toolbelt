using JosephM.Xrm.Vsix.Application;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.RefreshSchema
{
    public class MenuItemVisibleSchemaCs : MenuItemVisible
    {
        public override bool IsVisible(IVisualStudioService visualStudioService)
        {
            var selectedItems = visualStudioService.GetSelectedFileNamesQualified();
            return selectedItems.Count() == 1
                && selectedItems.First().ToLower().EndsWith("schema.cs");
        }
    }
}
