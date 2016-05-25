using System.Collections.Generic;
using System.IO;
using EnvDTE;
using EnvDTE80;
using JosephM.Core.Serialisation;
using JosephM.Core.Utility;

namespace JosephM.XRM.VSIX.Utilities
{
    public class VisualStudioService : IVisualStudioService
    {
        private DTE2 DTE { get; set; }

        public VisualStudioService(DTE2 dte)
        {
            DTE = dte;
        }

        public VisualStudioService(DTE2 dte, string useSolutionDirectory)
            : this(dte)
        {
            OverrideSolutionDirectory = useSolutionDirectory;
        }

        private string OverrideSolutionDirectory { get; set; }

        public Solution2 Solution
        {
            get { return DTE.Solution as Solution2; }
        }

        public string SolutionDirectory
        {
            get
            {
                if(!string.IsNullOrWhiteSpace(OverrideSolutionDirectory))
                    return OverrideSolutionDirectory;
                var fileInfo = new FileInfo(Solution.FullName);
                return fileInfo.DirectoryName;
            }
        }

        public string AddSolutionItem(string name, string serialised)
        {
            var project = VsixUtility.AddSolutionFolder(Solution, "SolutionItems");
            var solutionItemsFolder = SolutionDirectory + @"\SolutionItems";
            FileUtility.WriteToFile(solutionItemsFolder, name, serialised);
            VsixUtility.AddProjectItem(project.ProjectItems, Path.Combine(solutionItemsFolder, name));
            return Path.Combine(solutionItemsFolder, name);
        }


        public string AddSolutionItem<T>(string name, T objectToSerialise)
        {
            var json = JsonHelper.ObjectToJsonString(objectToSerialise);
            return AddSolutionItem(name, json);
        }

        public IEnumerable<IVisualStudioProject> GetSolutionProjects()
        {
            var projects = new List<IVisualStudioProject>();
            foreach(Project project in Solution.Projects)
                projects.Add(item: new VisualStudioProject(project));
            return projects;
        }

        public class VisualStudioProject : IVisualStudioProject
        {
            private Project _project;

            public VisualStudioProject(Project project)
            {
                _project = project;
            }

            public string Name { get { return _project.Name; } }
            public IProjectItem AddProjectItem(string file)
            {
                return new VisualStudioProjectItem(VsixUtility.AddProjectItem(_project.ProjectItems, file));
            }

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
            }
        }

        public void CloseAllDocuments()
        {
            DTE.Documents.CloseAll();
        }
    }
}
