using EnvDTE;
using JosephM.Xrm.Vsix.Utilities;

namespace JosephM.XRM.VSIX.Utilities
{
    public class VisualStudioItem : IVisualStudioItem
    {
        public SelectedItem Item { get; set; }

        public VisualStudioItem(SelectedItem item)
        {
            Item = item;
        }
    }
}