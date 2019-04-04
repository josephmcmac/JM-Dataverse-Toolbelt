using JosephM.Application.Application;
using JosephM.Xrm.Vsix.Application;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.AppConfig;

namespace JosephM.Xrm.Vsix.Module.ImportRecords
{
    public class MenuItemVisibleImportRecords : MenuItemVisibleForFileTypes
    {
        public override IEnumerable<string> ValidExtentions => new[] { "xml" };

        public override bool IsVisible(IApplicationController applicationController)
        {
            var visualStudioService = applicationController.ResolveType<IVisualStudioService>();
            var selectedItems = visualStudioService.GetSelectedItems();
            return selectedItems.All(si => si.FileName?.Contains("Data") ?? false)
                && base.IsVisible(applicationController);
        }
    }
}
