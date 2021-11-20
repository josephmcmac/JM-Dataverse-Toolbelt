using EnvDTE;
using System;
using System.Collections.Generic;
using System.IO;

namespace JosephM.Xrm.Vsix.Application
{
    public class VisualStudioProjectBase
    {
        protected Project Project { get; set; }

        public VisualStudioProjectBase(Project project)
        {
            Project = project;
        }

        public string Name { get { return Project.Name; } }

        public IEnumerable<IProjectItem> ProjectItems
        {
            get
            {
                if (Project?.ProjectItems == null)
                    return new IProjectItem[0];
                var results = new List<IProjectItem>();
                foreach (ProjectItem item in Project?.ProjectItems)
                {
                    results.Add(new VisualStudioProjectItem(item));
                }
                return results;
            }
        }

        public IProjectItem AddProjectItem(string file)
        {
            var fileInfo = new FileInfo(file);
            var fileName = fileInfo.Name;

            foreach (var item in ProjectItems)
            {
                if (item.Name == fileName)
                {
                    var itemFileName = item.FileName;
                    if (itemFileName.ToLower() != file.ToLower())
                        throw new Exception($"Error saving the settings file in the solution. The file referenced in the solution item does not match the expected location for the settings file. The expected path is {file}, the actual path is {itemFileName}. You will need to remove the file with the incorrect reference out of the solution");
                    return item;
                }
            }
            var newItem = Project.ProjectItems.AddFromFile(file);
            if (newItem.IsOpen)
            {
                var document = newItem.Document;
                if (document != null)
                {
                    document.Close();
                }
            }
            return new VisualStudioProjectItem(newItem);
        }
    }
}
