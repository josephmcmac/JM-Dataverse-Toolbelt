using EnvDTE;
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
                    return item;
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
