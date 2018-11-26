using JosephM.Core.Utility;
using JosephM.Xrm.Vsix.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.Vsix.Test
{
    public class FakeVisualStudioSolutionFolder : ISolutionFolder
    {
        public FakeVisualStudioSolutionFolder(string folderPath)
        {
            FolderPath = folderPath;
        }

        private string FolderPath { get; set; }

        public string Name
        {
            get
            {
                var folderInfo = new DirectoryInfo(FolderPath);
                return folderInfo.Name;
            }
        }

        public string ParentProjectName
        {
            get
            {
                var folderInfo = new DirectoryInfo(FolderPath);
                return folderInfo.Parent?.Name;
            }
        }

        public IEnumerable<IProjectItem> ProjectItems
        {
            get
            {
                return FileUtility.GetFiles(FolderPath).Select(f => new FakeVisualStudioProjectItem(f)).ToArray();
            }
        }

        public IEnumerable<ISolutionFolder> SubFolders
        {
            get
            {
                return FileUtility.GetFolders(FolderPath).Select(f => new FakeVisualStudioSolutionFolder(f)).ToArray();
            }
        }

        //these shouldn't need adding as we just track on disk
        public IProjectItem AddProjectItem(string file)
        {
            return new FakeVisualStudioProjectItem(file);
        }

        public ISolutionFolder AddSubFolder(string subFolder)
        {
            return new FakeVisualStudioSolutionFolder(subFolder);
        }

        public void CopyFilesIntoSolutionFolder(string folderDirectory)
        {

        }
    }
}
