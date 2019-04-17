using System.Collections.Generic;

namespace JosephM.Xrm.Vsix.Application
{
    public interface IVisualStudioService
    {
        string SolutionDirectory { get; }
        string AddVsixSetting(string name, string serialised);
        string AddVsixSetting<T>(string name, T objectToSerialise);
        string AddVsixSetting(string fileQualified);
        IVisualStudioProject GetProject(string name);
        void AddFolder(string folderDirectory);
        string GetVsixSettingText(string name);
        string GetItemText(string name, string folderName);
        string BuildSelectedProjectAndGetAssemblyName();
        string GetSelectedProjectAssemblyName();
        IEnumerable<string> GetSelectedFileNamesQualified();
        void SaveSelectedFiles();
        IEnumerable<IVisualStudioItem> GetSelectedItems();
        ISolutionFolder GetSolutionFolder(string solutionFolderName);
        string ItemFolderName { get; }
        string GetSolutionName();
        IEnumerable<IVisualStudioProject> GetProjects();
    }

    public interface IVisualStudioProject
    {
        string Name { get; }
        IProjectItem AddProjectItem(string file);
        void AddItem(string fileName, string fileContent, params string[] folderPath);
    }

    public interface IProjectItem : IVisualStudioItem
    {
        void SetProperty(string propertyName, object value);
        string FileFolder { get; }
    }

    public interface ISolutionFolder : IVisualStudioItem
    {
        string ParentProjectName { get; }
        IEnumerable<IProjectItem> ProjectItems { get; }
        IEnumerable<ISolutionFolder> SubFolders { get; }
        IProjectItem AddProjectItem(string file);
        ISolutionFolder AddSubFolder(string subFolder);
        void CopyFilesIntoSolutionFolder(string folderDirectory);
    }

    public interface IVisualStudioItem
    {
        string Name { get; }
        string NameOfContainingProject { get; }
        string FileName { get; }
    }
}