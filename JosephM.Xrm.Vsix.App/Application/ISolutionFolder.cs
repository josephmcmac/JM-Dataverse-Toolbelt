using System.Collections.Generic;

namespace JosephM.Xrm.Vsix.Application
{
    public interface ISolutionFolder : IVisualStudioItem
    {
        string ParentProjectName { get; }
        IEnumerable<IProjectItem> ProjectItems { get; }
        IEnumerable<ISolutionFolder> SubFolders { get; }
        IProjectItem AddProjectItem(string file);
        ISolutionFolder AddSubFolder(string subFolder);
        void CopyFilesIntoSolutionFolder(string folderDirectory);
    }
}