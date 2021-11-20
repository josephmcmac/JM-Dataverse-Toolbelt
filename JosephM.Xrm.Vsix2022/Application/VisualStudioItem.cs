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

        public string NameOfContainingProject
        {
            get
            {
                return Item?.ProjectItem?.ContainingProject?.Name;
            }
        }

        public string FileName
        {
            get
            {
                return Item.ProjectItem != null
                    && Item.ProjectItem.FileCount > 0
                    ? Item.ProjectItem.FileNames[1]
                    : null;
            }
        }
    }
}