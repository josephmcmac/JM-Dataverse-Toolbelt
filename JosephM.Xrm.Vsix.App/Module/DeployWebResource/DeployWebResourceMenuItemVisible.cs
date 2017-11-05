using System.Collections.Generic;

namespace JosephM.Xrm.Vsix.Module.DeployWebResource
{
    public class DeployWebResourceMenuItemVisible : MenuItemVisibleForFileTypes
    {
        public override IEnumerable<string> ValidExtentions => DeployWebResourceService.WebResourceTypes.Keys;
    }
}