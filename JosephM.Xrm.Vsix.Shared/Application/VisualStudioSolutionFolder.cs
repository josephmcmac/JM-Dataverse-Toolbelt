using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;

namespace JosephM.Xrm.Vsix.Application
{
    public class VisualStudioSolutionFolder : VisualStudioProjectBase, ISolutionFolder
    {

        private SolutionFolder SolutionFolder { get { return Project?.Object as SolutionFolder ?? throw new NullReferenceException("Could not find SolutionFolder object"); } }

        public VisualStudioSolutionFolder(Project project)
            : base(project)
        {
        }

        public string ParentProjectName
        {
            get
            {
                return SolutionFolder?.Parent?.ParentProjectItem?.ContainingProject?.Name;
            }
        }

        public IEnumerable<ISolutionFolder> SubFolders
        {
            get
            {
                if (Project?.ProjectItems == null)
                    return new VisualStudioSolutionFolder[0];
                var results = new List<VisualStudioSolutionFolder>();
                foreach (ProjectItem item in Project?.ProjectItems)
                {
                    if (item.SubProject != null)
                        results.Add(new VisualStudioSolutionFolder(item.SubProject));
                }
                return results;
            }
        }

        public ISolutionFolder AddSubFolder(string subFolder)
        {
            var parent = SolutionFolder.Parent;

            foreach (ProjectItem item in parent.ProjectItems)
            {
                if (item.Name == subFolder)
                    return new VisualStudioSolutionFolder((Project)item.Object);
            }
            return new VisualStudioSolutionFolder(SolutionFolder.AddSolutionFolder(subFolder));
        }

        public void CopyFilesIntoSolutionFolder(string folderDirectory)
        {
            var parent = SolutionFolder.Parent;
            foreach (var file in Directory.GetFiles(folderDirectory))
            {
                string existingFileName = string.Empty;
                var projectItemName = string.Empty;
                var fileCount = 0;
                try
                {
                    var alreadyExists = false;
                    foreach(ProjectItem projectItem in parent.ProjectItems)
                    {
                        var visualStudioProjectItem = new VisualStudioProjectItem(projectItem);
                        existingFileName = visualStudioProjectItem.FileName;
                        if (existingFileName != null && existingFileName.ToLower() == file.ToLower())
                        {
                            alreadyExists = true;
                            break;
                        }
                    }
                    if (!alreadyExists)
                    {
                        parent.ProjectItems.AddFromFile(file);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error adding file '{file}' to solution folder. projectItemName={projectItemName}. existingFileName={existingFileName}. fileCount={fileCount}. {ex.Message}", ex);
                }
            }
            foreach (var childFolder in Directory.GetDirectories(folderDirectory))
            {
                var childSolutionFolder = AddSolutionFolderSubFolder(new DirectoryInfo(childFolder).Name);
                childSolutionFolder.CopyFilesIntoSolutionFolder(childFolder);
            }
        }

        private ISolutionFolder AddSolutionFolderSubFolder(string folder)
        {
            var parent = SolutionFolder.Parent;

            foreach (ProjectItem item in parent.ProjectItems)
            {
                if (item.Name == folder)
                    return new VisualStudioSolutionFolder((Project)item.Object);
            }
            return new VisualStudioSolutionFolder(SolutionFolder.AddSolutionFolder(folder));
        }

        public string NameOfContainingProject
        {
            get
            {
                return null;
            }
        }

        string IVisualStudioItem.FileName => null;
    }
}