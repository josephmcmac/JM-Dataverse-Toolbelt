using EnvDTE;
using System;
using System.IO;

namespace JosephM.Xrm.Vsix.Application
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
            foreach (Property prop in ProjectItem.Properties)
            {
                if (prop.Name == propertyName)
                {
                    prop.Value = value;
                    return;
                }
            }
            throw new NullReferenceException(string.Format("Could not find property {0}", propertyName));
        }

        public string FileName
        {
            get
            {
                //the com api has been found too inconsistent and not reliable in terms of what would normally be expected
                //so here I'll just check if the 0 or 1 index returns a file name
                string fileName = null;
                try
                {
                    fileName = ProjectItem.FileNames[1];
                }
                catch (Exception) { }
                if (fileName == null)
                {
                    try
                    {
                        fileName = ProjectItem.FileNames[0];
                    }
                    catch (Exception) { }
                }
                return fileName;
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

        public string NameOfContainingProject
        {
            get
            {
                return ProjectItem?.ContainingProject?.Name;
            }
        }
    }
}
