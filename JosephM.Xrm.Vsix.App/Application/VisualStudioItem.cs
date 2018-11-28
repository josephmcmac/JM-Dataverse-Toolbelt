using EnvDTE;

namespace JosephM.Xrm.Vsix.Application
{
    public class VisualStudioItem : IVisualStudioItem
    {
        public SelectedItem Item { get; set; }

        public string Name => Item.Name;

        public VisualStudioItem(SelectedItem item)
        {
            Item = item;
        }
    }
}