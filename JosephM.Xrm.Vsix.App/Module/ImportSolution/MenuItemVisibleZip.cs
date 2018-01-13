using System.Collections.Generic;

namespace JosephM.Xrm.Vsix.Module.ImportSolution
{
    public class MenuItemVisibleZip : MenuItemVisibleForFileTypes
    {
        public override IEnumerable<string> ValidExtentions => new[] { "zip" };
    }
}
