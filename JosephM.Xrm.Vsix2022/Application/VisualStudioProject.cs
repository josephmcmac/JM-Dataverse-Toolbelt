using EnvDTE;
using JosephM.Core.Utility;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Application
{
    public class VisualStudioProject : VisualStudioProjectBase, IVisualStudioProject
    {
        public VisualStudioProject(Project project)
            : base(project)
        {
        }

        public string GetProperty(string name)
        {
            foreach (Property prop in Project.Properties)
            {
                if (prop.Name == name)
                {
                    return prop.Value == null ? null : prop.Value.ToString();
                }
            }
            return null;
        }

        public void AddItem(string fileName, string fileContent, params string[] folderPath)
        {
            var projectItems = Project.ProjectItems;
            if (folderPath != null)
            {
                foreach (var path in folderPath)
                {
                    ProjectItem thisPartProjectItem = null;
                    foreach (ProjectItem item in projectItems)
                    {
                        if (item.Name?.ToLower() == path?.ToLower())
                        {
                            thisPartProjectItem = item;

                        }
                    }
                    if (thisPartProjectItem == null)
                        thisPartProjectItem = projectItems.AddFolder(path);
                    projectItems = thisPartProjectItem.ProjectItems;
                }
            }
            var projectFileName = Project.FileName;
            var projectDirectory = new FileInfo(projectFileName).Directory;
            var fileDirectory = projectDirectory.FullName;
            if (folderPath != null && folderPath.Any())
                fileDirectory = Path.Combine(fileDirectory, Path.Combine(folderPath));
            FileUtility.WriteToFile(fileDirectory, fileName, fileContent);
            projectItems.AddFromFile(Path.Combine(fileDirectory, fileName));
        }
    }
}
