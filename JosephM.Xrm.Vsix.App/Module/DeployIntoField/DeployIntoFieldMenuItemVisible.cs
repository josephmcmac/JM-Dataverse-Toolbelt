using System.Collections.Generic;

namespace JosephM.Xrm.Vsix.Module.DeployIntoField
{
    public class DeployIntoFieldMenuItemVisible : MenuItemVisibleForFileTypes
    {
        public override IEnumerable<string> ValidExtentions => DeployIntoFieldService.IntoFieldTypes;
    }
}