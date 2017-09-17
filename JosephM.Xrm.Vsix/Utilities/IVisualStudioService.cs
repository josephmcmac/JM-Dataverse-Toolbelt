using System.Collections.Generic;
using EnvDTE;

namespace JosephM.XRM.VSIX.Utilities
{
    public interface IVisualStudioService
    {
        string SolutionDirectory { get; }
        string AddSolutionItem(string connectionFileName, string serialised);
        string AddSolutionItem<T>(string name, T objectToSerialise);
        string AddSolutionItem(string fileQualified);
        IEnumerable<IVisualStudioProject> GetSolutionProjects();
        void AddFolder(string folderDirectory);
    }

    public interface IVisualStudioProject
    {
        string Name { get; }
        IProjectItem AddProjectItem(string file);
    }

    public interface IProjectItem
    {
        void SetProperty(string propertyName, object value);
    }
}