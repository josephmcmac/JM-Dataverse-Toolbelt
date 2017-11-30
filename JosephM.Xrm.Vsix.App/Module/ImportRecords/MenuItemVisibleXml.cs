using System.Collections.Generic;

namespace JosephM.Xrm.Vsix.Module.ImportRecords
{
    public class MenuItemVisibleXml : MenuItemVisibleForFileTypes
    {
        public override IEnumerable<string> ValidExtentions => new[] { "xml" };
    }
}
