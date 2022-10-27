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
        bool? IsCurrentlyInstalledInTarget { get; set; }
        bool? CurrentTargetVersionManaged { get; set; }
        string CurrentTargetVersion { get; set; }
        bool InstallAsUpgrade { get; set; }
        bool IsInstallingNewerVersion { get; }
    }
}
