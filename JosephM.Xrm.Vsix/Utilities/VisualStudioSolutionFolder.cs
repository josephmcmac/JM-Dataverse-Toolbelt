using EnvDTE;
using EnvDTE80;
using System;
using System.Collections.Generic;
using System.IO;

namespace JosephM.Xrm.Vsix.Utilities
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
            var name = parent.Name;
            foreach (var file in Directory.GetFiles(folderDirectory))
            {
                parent.ProjectItems.AddFromFile(file);
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
    }
}