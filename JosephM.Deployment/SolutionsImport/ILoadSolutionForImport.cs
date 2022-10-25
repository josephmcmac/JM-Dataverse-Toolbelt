using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Deployment.SolutionsImport
{
    public interface ILoadSolutionForImport
    {
        SavedXrmRecordConfiguration TargetConnection { get; set; }
        bool? IsManaged { get; set; }
        string Version { get; set; }
        string UniqueName { get; set; }
        string FriendlyName { get; set; }
        bool? IsCurrentlyInstalled { get; set; }
        bool? CurrentVersionManaged { get; set; }
        string CurrentVersion { get; set; }
    }
}
