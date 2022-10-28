using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.SolutionsImport;
using JosephM.XrmModule.SavedXrmConnections;
using System.IO;

namespace JosephM.Deployment.DeployPackage
{
    [Group(Sections.SolutionDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 30, displayLabel: false)]
    [Group(Sections.CurrentSolutionDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 40, displayLabel: false)]
    [Group(Sections.InstallOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 50)]
    public class DeployPackageSolutionImportItem : IImportSolutionsRequestItem, ILoadSolutionForImport
    {
        public DeployPackageSolutionImportItem()
        {
            OverwriteCustomisations = true;
        }

        public SavedXrmRecordConfiguration TargetConnection { get; set; }

        [Hidden]
        public FileReference SolutionZip { get; set; }

        [GridWidth(300)]
        [GridField]
        [GridReadOnly]
        [Group(Sections.SolutionDetails)]
        [DisplayOrder(100)]
        [DisplayName("Solution Name")]
        public string FriendlyName { get; set; }

        [GridWidth(110)]
        [GridField]
        [GridReadOnly]
        [Group(Sections.SolutionDetails)]
        [DisplayOrder(110)]
        public string Version { get; set; }

        [GridWidth(110)]
        [GridField]
        [GridReadOnly]
        [Group(Sections.SolutionDetails)]
        [DisplayOrder(120)]
        public bool? IsManaged { get; set; }

        [Hidden]
        public string UniqueName { get; set; }

        [GridWidth(110)]
        [GridField]
        [GridReadOnly]
        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(200)]
        public bool? IsCurrentlyInstalledInTarget { get; set; }

        [GridWidth(110)]
        [GridField]
        [GridReadOnly]
        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(210)]
        public string CurrentTargetVersion { get; set; }

        [GridWidth(110)]
        [GridField]
        [GridReadOnly]
        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(220)]
        public bool? CurrentTargetVersionManaged { get; set; }

        [Hidden]
        public bool IsInstallingNewerVersion
        {
            get
            {
                return !string.IsNullOrWhiteSpace(CurrentTargetVersion) && !string.IsNullOrWhiteSpace(Version) && VersionHelper.IsNewerVersion(Version, CurrentTargetVersion);
            }
        }

        [GridWidth(110)]
        [GridField]
        [Group(Sections.InstallOptions)]
        [DisplayOrder(300)]
        public bool OverwriteCustomisations { get; set; }

        [PropertyInContextByPropertyValue(nameof(IsInstallingNewerVersion), true)]
        [PropertyInContextByPropertyValue(nameof(CurrentTargetVersionManaged), true)]
        [PropertyInContextByPropertyValue(nameof(IsManaged), true)]
        [GridWidth(110)]
        [GridField]
        [Group(Sections.InstallOptions)]
        [DisplayOrder(310)]
        public bool InstallAsUpgrade { get; set; }

        byte[] IImportSolutionsRequestItem.GetSolutionZipContent()
        {
            return SolutionZip == null
                    ? null
                    : File.ReadAllBytes(SolutionZip.FileName);
        }

        private static class Sections
        {
            public const string SolutionDetails = "Solution Details";
            public const string CurrentSolutionDetails = "Current Solution Details";
            public const string InstallOptions = "Install Options";
        }
    }
}