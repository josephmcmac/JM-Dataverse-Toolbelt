using System.Collections.Generic;

namespace JosephM.Xrm.Vsix.Module.CustomisationImport
{
    public class MenuItemVisibleXlsx : MenuItemVisibleForFileTypes
    {
        public override IEnumerable<string> ValidExtentions => new[] { "xlsx" };
    }
}
