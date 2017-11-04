using EnvDTE;

namespace JosephM.Xrm.Vsix.Application
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