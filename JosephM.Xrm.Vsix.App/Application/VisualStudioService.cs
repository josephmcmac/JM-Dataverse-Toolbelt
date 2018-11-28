using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;

namespace JosephM.Xrm.Vsix.Application
{
    public partial class VisualStudioService : VisualStudioServiceBase
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

        private Solution2 Solution
        {
            get { return DTE.Solution as Solution2; }
        }

        public override string SolutionDirectory
        {
            get
            {
                if(!string.IsNullOrWhiteSpace(OverrideSolutionDirectory))
                    return OverrideSolutionDirectory;
                var fileInfo = new FileInfo(Solution.FullName);
                return fileInfo.DirectoryName;
            }
        }

        protected override ISolutionFolder AddSolutionFolder(string folder)
        {
            foreach (Project item in Solution.Projects)
            {
                if (item.Name == folder)
                    return new VisualStudioSolutionFolder(item);
            }
            return new VisualStudioSolutionFolder(Solution.AddSolutionFolder(folder));
        }

        public override IVisualStudioProject GetProject(string name)
        {
            foreach (Project project in Solution.Projects)
            {
                if (project.Name == name)
                    return new VisualStudioProject(project);
            }
            throw new NullReferenceException("Couldnt find project named " + name);
        }

        public void CloseAllDocuments()
        {
            DTE.Documents.CloseAll();
        }

        public override void AddFolder(string folderDirectory)
        {
            base.AddFolder(folderDirectory);
            if (!Solution.Saved)
                Solution.SaveAs(Solution.FullName);
        }

        public override ISolutionFolder GetSolutionFolder(string solutionFolderName)
        {
            var project = GetProject(DTE.Solution as Solution2, solutionFolderName);
            return project == null ? null : new VisualStudioSolutionFolder(project);
        }

        private static Project GetProject(Solution2 solution, string name)
        {
            if (solution.Projects != null)
            {
                foreach (Project item in solution.Projects)
                {
                    if (item.Name == name)
                        return item;
                }
            }
            return null;
        }

        public override string GetSelectedProjectAssemblyName()
        {
            var selectedItems = DTE.SelectedItems;
            foreach (SelectedItem item in selectedItems)
            {
                var project = item.Project;
                if (project.Name != null)
                {
                    return GetProperty(project.Properties, "AssemblyName");
                }
            }
            throw new NullReferenceException("Could not find assembly name for selected project");
        }

        public override IEnumerable<string> GetSelectedFileNamesQualified()
        {
            var fileNames = new List<string>();

            var items = DTE.SelectedItems;
            foreach (SelectedItem item in items)
            {
                if (item.ProjectItem != null && !string.IsNullOrWhiteSpace(item.Name))
                {
                    string fileName = null;
                    try
                    {
                        fileName = item.ProjectItem.FileNames[0];
                    }
                    catch (Exception) { }
                    if (fileName == null)
                        try
                        {
                            fileName = item.ProjectItem.FileNames[1];
                        }
                        catch (Exception) { }
                    if (fileName == null)
                        throw new Exception("Could not extract file name for ProjectItem " + item.Name);
                    fileNames.Add(fileName);
                }
            }

            return fileNames;
        }

        public override IEnumerable<IVisualStudioItem> GetSelectedItems()
        {
            var results = new List<IVisualStudioItem>();
            var selectedItems = DTE.SelectedItems;
            foreach (SelectedItem item in selectedItems)
            {
                if (item.Project != null && item.Project.Object != null && item.Project.Object is SolutionFolder)
                {
                    results.Add(new VisualStudioSolutionFolder(item.Project));
                }
                else
                {
                    results.Add(new VisualStudioItem(item));
                }
            }
            return results;
        }

        public override string BuildSelectedProjectAndGetAssemblyName()
        {
            var selectedProject = GetSelectedProject();
            var build = DTE.Solution.SolutionBuild;
            build.BuildProject(selectedProject.ConfigurationManager.ActiveConfiguration.ConfigurationName, selectedProject.UniqueName, WaitForBuildToFinish: true);
            var info = build.LastBuildInfo;

            if (info == 0)
            {
                var assemblyName = GetProperty(selectedProject.Properties, "AssemblyName");
                var outputPath =
                    GetProperty(selectedProject.ConfigurationManager.ActiveConfiguration.Properties,
                        "OutputPath");
                var fileInfo = new FileInfo(selectedProject.FullName);
                var rootFolder = fileInfo.DirectoryName;
                var outputFolder = Path.Combine(rootFolder ?? "", outputPath);
                var assemblyFile = Path.Combine(outputFolder, assemblyName) + ".dll";
                return assemblyFile;
            }
            return null;
        }

        private Project GetSelectedProject()
        {
            var selectedItems = DTE.SelectedItems;
            foreach (SelectedItem item in selectedItems)
            {
                var project = item.Project;
                if (project.Name != null)
                {
                    return project;
                }
            }
            throw new NullReferenceException("Could not identify a selected project");
        }

        private string GetProperty(Properties properties, string name)
        {
            foreach (Property prop in properties)
            {
                if (prop.Name == name)
                {
                    return prop.Value == null ? null : prop.Value.ToString();
                }
            }
            return null;
        }
    }
}
