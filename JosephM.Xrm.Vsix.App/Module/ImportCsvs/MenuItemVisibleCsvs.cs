using System.Collections.Generic;

namespace JosephM.Xrm.Vsix.Module.ImportCsvs
{
    public class MenuItemVisibleCsvs : MenuItemVisibleForFileTypes
    {
        public override IEnumerable<string> ValidExtentions => new[] { "csv" };
    }
}
