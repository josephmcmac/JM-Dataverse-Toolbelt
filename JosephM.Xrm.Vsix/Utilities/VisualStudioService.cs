using System.Collections.Generic;
using System.IO;
using EnvDTE;
using EnvDTE80;
using JosephM.Core.Serialisation;
using JosephM.Core.Utility;
using System;
using System.Linq;

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

        private Project AddSolutionFolder(string folder)
        {
            foreach (Project item in Solution.Projects)
            {
                if (item.Name == folder)
                    return item;
            }
            return Solution.AddSolutionFolder(folder);
        }

        public string AddSolutionItem(string name, string serialised)
        {
            var project = AddSolutionFolder("SolutionItems");
            var solutionItemsFolder = SolutionDirectory + @"\SolutionItems";
            FileUtility.WriteToFile(solutionItemsFolder, name, serialised);
            VsixUtility.AddProjectItem(project.ProjectItems, Path.Combine(solutionItemsFolder, name));
            return Path.Combine(solutionItemsFolder, name);
        }

        public string AddSolutionItem(string fileQualified)
        {
            var project = AddSolutionFolder("SolutionItems");
            if (fileQualified.StartsWith(SolutionDirectory))
            {
                var subString = fileQualified.Substring(SolutionDirectory.Length + 1);
                if (subString.Contains(@"\"))
                {
                    var folder = subString.Substring(0, subString.LastIndexOf(@"\"));
                    FileUtility.CheckCreateFolder(SolutionDirectory + @"\" + folder);
                    project = AddSolutionFolder(folder);
                }
            }
            VsixUtility.AddProjectItem(project.ProjectItems, fileQualified);
            return fileQualified;
        }

        public string AddSolutionItem<T>(string name, T objectToSerialise)
        {
            var json = JsonHelper.ObjectAsTypeToJsonString(objectToSerialise);
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

        public void AddFolder(string folderDirectory)
        {
            if (folderDirectory == null)
                throw new ArgumentNullException(nameof(folderDirectory));

            var solutionDirectory = SolutionDirectory;

            if (!folderDirectory.StartsWith(solutionDirectory))
                throw new ArgumentOutOfRangeException(nameof(folderDirectory), "Required to be in solution directory - " + solutionDirectory);

            var subPath = folderDirectory.Substring(solutionDirectory.Length);

            var subDirectories = subPath.Split(Path.DirectorySeparatorChar).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            Project carryProject = null;
            foreach(var item in subDirectories)
            {
                if (carryProject == null)
                {
                    carryProject = AddSolutionFolder(item);
                }
                else
                {
                    var solutionFolder = (SolutionFolder)carryProject.Object;
                    carryProject = AddSolutionFolderSubFolder(solutionFolder, item);
                }
            }
            var releaseSolutionFolder = (SolutionFolder)carryProject.Object;
            CopyFilesIntoSolutionFolder(releaseSolutionFolder, folderDirectory);

            if (!Solution.Saved)
                Solution.SaveAs(Solution.FullName);
        }

        private Project AddSolutionFolderSubFolder(SolutionFolder solutionFolder, string folder)
        {
            var parent = solutionFolder.Parent;

            foreach (ProjectItem item in parent.ProjectItems)
            {
                if (item.Name == folder)
                    return (Project)item.Object;
            }
            return solutionFolder.AddSolutionFolder(folder);
        }

        private void CopyFilesIntoSolutionFolder(SolutionFolder releaseSolutionFolder, string folderDirectory)
        {
            var parent = releaseSolutionFolder.Parent;
            var name = parent.Name;
            foreach (var file in Directory.GetFiles(folderDirectory))
            {
                parent.ProjectItems.AddFromFile(file);
            }
            foreach (var childFolder in Directory.GetDirectories(folderDirectory))
            {
                var childSolutionFolder = AddSolutionFolderSubFolder(releaseSolutionFolder, new DirectoryInfo(childFolder).Name);
                CopyFilesIntoSolutionFolder((SolutionFolder)childSolutionFolder.Object, childFolder);
            }
        }

        public string GetSolutionItemText(string name)
        {
            string fileName = null;
            var solutionItems = GetProject(DTE.Solution as Solution2, "SolutionItems");
            if (solutionItems == null)
                return null;
            foreach (ProjectItem item in solutionItems.ProjectItems)
            {
                if (item.Name == name)
                {
                    fileName = item.FileNames[1];
                }
            }
            if (fileName == null)
                return null;
            return File.ReadAllText(fileName);
        }


        private static Project GetProject(Solution2 solution, string name)
        {
            foreach (Project item in solution.Projects)
            {
                if (item.Name == name)
                    return item;
            }
            return null;
        }
    }
}
