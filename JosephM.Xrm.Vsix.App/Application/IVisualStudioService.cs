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
        string BuildSelectedProjectAndGetAssemblyName(bool addIlMergePath);
        string GetSelectedProjectAssemblyName();
        IEnumerable<string> GetSelectedFileNamesQualified();
        void SaveSelectedFiles();
        IEnumerable<IVisualStudioItem> GetSelectedItems();
        ISolutionFolder GetSolutionFolder(string solutionFolderName);
        string ItemFolderName { get; }
        string GetSolutionName();
        IEnumerable<IVisualStudioProject> GetProjects();
    }
}