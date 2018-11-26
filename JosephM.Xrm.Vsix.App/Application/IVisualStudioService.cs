using System.Collections.Generic;

namespace JosephM.Xrm.Vsix.Application
{
    public interface IVisualStudioService
    {
        string SolutionDirectory { get; }
        string AddSolutionItem(string name, string serialised);
        string AddSolutionItem<T>(string name, T objectToSerialise);
        string AddSolutionItem(string fileQualified);
        IEnumerable<IVisualStudioProject> GetSolutionProjects();
        void AddFolder(string folderDirectory);
        string GetSolutionItemText(string name);
        string BuildSelectedProjectAndGetAssemblyName();
        string GetSelectedProjectAssemblyName();
        IEnumerable<string> GetSelectedFileNamesQualified();
        IEnumerable<IVisualStudioItem> GetSelectedItems();
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