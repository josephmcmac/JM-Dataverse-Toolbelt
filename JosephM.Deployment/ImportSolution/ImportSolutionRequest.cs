using JosephM.Core.Attributes;
using JosephM.Core.Constants;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.SolutionsImport;
using JosephM.XrmModule.SavedXrmConnections;
using System.IO;
using System.Windows.Navigation;

namespace JosephM.Deployment.ImportSolution
{
    [Group(Sections.Connection, Group.DisplayLayoutEnum.VerticalCentered, order: 10, displayLabel: false)]
    [Group(Sections.SolutionFile, Group.DisplayLayoutEnum.VerticalCentered, order: 20, displayLabel: false)]
    [Group(Sections.SolutionDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 30, displayLabel: false)]
    [Group(Sections.CurrentSolutionDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 40, displayLabel: false)]
    [Group(Sections.InstallOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 50)]
    public class ImportSolutionRequest : ServiceRequestBase, IImportSolutionsRequestItem, ILoadSolutionForImport
    {
        public static ImportSolutionRequest CreateForImportSolution(string file)
        {
            return new ImportSolutionRequest()
            {
                SolutionZip = new FileReference(file),
                HideSolutionFile = true
            };
        }

        public ImportSolutionRequest()
        {
            OverwriteCustomisations = true;
        }

        [DoNotAllowAdd]
        [Group(Sections.Connection)]
        [DisplayName("Saved connection instance to import into")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        public SavedXrmRecordConfiguration TargetConnection { get; set; }

        [Hidden]
        public bool HideSolutionFile { get; set; }

        [Group(Sections.SolutionFile)]
        [RequiredProperty]
        [FileMask(FileMasks.ZipFile)]
        [PropertyInContextByPropertyValue(nameof(HideSolutionFile), false)]
        public FileReference SolutionZip { get; set; }

        [Group(Sections.SolutionDetails)]
        [DisplayOrder(100)]
        [ReadOnlyWhenSet]
        [PropertyInContextByPropertyNotNull(nameof(FriendlyName))]
        [DisplayName("Solution Name")]
        public string FriendlyName { get; set; }

        [Group(Sections.SolutionDetails)]
        [DisplayOrder(110)]
        [ReadOnlyWhenSet]
        [PropertyInContextByPropertyNotNull(nameof(Version))]
        public string Version { get; set; }

        [Group(Sections.SolutionDetails)]
        [DisplayOrder(120)]
        [ReadOnlyWhenSet]
        [PropertyInContextByPropertyNotNull(nameof(IsManaged))]
        public bool? IsManaged { get; set; }

        [Hidden]
        public string UniqueName { get; set; }

        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(200)]
        [ReadOnlyWhenSet]
        [PropertyInContextByPropertyNotNull(nameof(TargetConnection))]
        [PropertyInContextByPropertyValue(nameof(IsManaged), true)]
        [PropertyInContextByPropertyValue(nameof(IsCurrentlyInstalled), false)]
        public bool? IsCurrentlyInstalled { get; set; }

        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(210)]
        [ReadOnlyWhenSet]
        [PropertyInContextByPropertyValue(nameof(IsCurrentlyInstalled), true)]
        public string CurrentVersion { get; set; }

        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(220)]
        [ReadOnlyWhenSet]
        [PropertyInContextByPropertyValue(nameof(IsManaged), true)]
        [PropertyInContextByPropertyValue(nameof(IsCurrentlyInstalled), true)]
        [PropertyInContextByPropertyValue(nameof(CurrentVersionManaged), false)]
        [RequiredPropertyValue(true, "A managed solution cannot be installed into an instance where that solution is already unmanaged. Delete the solution from the target instance and try again")]
        public bool? CurrentVersionManaged { get; set; }

        [Hidden]
        public bool CurrentIsEarlierVersion
        {
            get
            {
                return !string.IsNullOrWhiteSpace(CurrentVersion) && !string.IsNullOrWhiteSpace(Version) && VersionHelper.IsNewerVersion(Version, CurrentVersion);
            }
        }

        [Group(Sections.InstallOptions)]
        [PropertyInContextByPropertyNotNull(nameof(FriendlyName))]
        [PropertyInContextByPropertyNotNull(nameof(SolutionZip))]
        [DisplayOrder(300)]
        public bool OverwriteCustomisations { get; set; }

        [Group(Sections.InstallOptions)]
        [DisplayOrder(310)]
        [PropertyInContextByPropertyValue(nameof(CurrentIsEarlierVersion), true)]
        [PropertyInContextByPropertyValue(nameof(CurrentVersionManaged), true)]
        [PropertyInContextByPropertyValue(nameof(IsManaged), true)]
        public bool InstallAsUpgrade { get; set; }

        byte[] IImportSolutionsRequestItem.GetSolutionZipContent()
        {
            return SolutionZip == null
                    ? null
                    : File.ReadAllBytes(SolutionZip.FileName);
        }

        private static class Sections
        {
            public const string Connection = "Connection";
            public const string SolutionFile = "Solution File";
            public const string SolutionDetails = "Solution Details";
            public const string CurrentSolutionDetails = "Current Solution Details";
            public const string InstallOptions = "Install Options";
        }
    }
}