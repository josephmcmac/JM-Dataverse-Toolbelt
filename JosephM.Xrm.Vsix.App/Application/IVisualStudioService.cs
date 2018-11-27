using System.Collections.Generic;

namespace JosephM.Xrm.Vsix.Application
{
    public interface IVisualStudioService
    {
        string SolutionDirectory { get; }
        string AddVsixSetting(string name, string serialised);
        string AddVsixSetting<T>(string name, T objectToSerialise);
        string AddVsixSetting(string fileQualified);
        IEnumerable<IVisualStudioProject> GetSolutionProjects();
        void AddFolder(string folderDirectory);
        string GetVsixSettingText(string name);
        string GetItemText(string name, string folderName);
        string BuildSelectedProjectAndGetAssemblyName();
        string GetSelectedProjectAssemblyName();
        IEnumerable<string> GetSelectedFileNamesQualified();
        IEnumerable<IVisualStudioItem> GetSelectedItems();
        ISolutionFolder GetSolutionFolder(string solutionFolderName);
        string ItemFolderName { get; }
    }

    public interface IVisualStudioProject
    {
        string Name { get; }
        IProjectItem AddProjectItem(string file);
    }

    public interface IProjectItem : IVisualStudioItem
    {
        void SetProperty(string propertyName, object value);
        string FileName { get; }
        string Name { get; }
        string FileFolder { get; }
    }

    public interface ISolutionFolder : IVisualStudioItem
    {
        string ParentProjectName { get; }

        string Name { get; }

        IEnumerable<IProjectItem> ProjectItems { get; }

        IEnumerable<ISolutionFolder> SubFolders { get; }

        IProjectItem AddProjectItem(string file);
        ISolutionFolder AddSubFolder(string subFolder);
        void CopyFilesIntoSolutionFolder(string folderDirectory);
    }

    public interface IVisualStudioItem
    {
    }
}