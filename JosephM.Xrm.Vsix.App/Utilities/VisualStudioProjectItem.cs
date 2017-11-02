using EnvDTE;
using JosephM.XRM.VSIX.Utilities;
using System.IO;

namespace JosephM.Xrm.Vsix.Utilities
{
    public class VisualStudioProjectItem : IProjectItem
    {
        public ProjectItem ProjectItem { get; set; }

        public VisualStudioProjectItem(ProjectItem projectItem)
        {
            ProjectItem = projectItem;
        }

        public void SetProperty(string propertyName, object value)
        {
            VsixUtility.SetProperty(ProjectItem.Properties, propertyName, value);
        }

        public bool HasFile
        {
            get
            {
                return ProjectItem.FileCount > 0;
            }
        }

        public string FileName
        {
            get
            {
                return HasFile ? ProjectItem.FileNames[1] : null;
            }
        }

        public string FileFolder
        {
            get
            {
                return FileName == null ? null : new FileInfo(FileName).DirectoryName;
            }
        }

        public string Name
        {
            get
            {
                return ProjectItem.Name;
            }
        }
    }
}
