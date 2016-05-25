using System.Collections.Generic;
using EnvDTE;

namespace JosephM.XRM.VSIX.Utilities
{
    public interface IVisualStudioService
    {
        string SolutionDirectory { get; }
        string AddSolutionItem(string connectionFileName, string serialised);
        string AddSolutionItem<T>(string name, T objectToSerialise);
        IEnumerable<IVisualStudioProject> GetSolutionProjects();
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