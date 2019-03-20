using JosephM.Application.Application;
using JosephM.Xrm.Vsix.Application;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module
{
    public abstract class MenuItemVisibleForFileTypes : MenuItemVisible
    {
        public abstract IEnumerable<string> ValidExtentions { get; }

        public override bool IsVisible(IApplicationController applicationController)
        {
            var visualStudioService = applicationController.ResolveType(typeof(IVisualStudioService)) as IVisualStudioService;
            var selectedItems = visualStudioService.GetSelectedFileNamesQualified();
            return selectedItems.All(f => ValidExtentions.Any(ext => f.EndsWith("." + ext)));
        }
    }
}
