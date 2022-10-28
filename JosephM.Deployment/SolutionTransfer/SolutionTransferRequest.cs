using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.SolutionsImport;
using JosephM.Record.Attributes;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Deployment.SolutionTransfer
{
    [Instruction("The solution will be exported from the source connection and imported into the target")]
    [AllowSaveAndLoad]
    [Group(Sections.Connection, Group.DisplayLayoutEnum.HorizontalLabelAbove, 10, displayLabel: false)]
    [Group(Sections.Solution, Group.DisplayLayoutEnum.HorizontalLabelAbove, 20, displayLabel: false)]
    [Group(Sections.ExportDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, 30, displayLabel: false)]
    [Group(Sections.CurrentSolutionDetails, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 50, displayLabel: false)]
    [Group(Sections.InstallOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 60)]
    public class SolutionTransferRequest : ServiceRequestBase, IImportSolutionsRequestItem, ILoadSolutionForImport
    {
        public SolutionTransferRequest()
        {
            OverwriteCustomisations = true;
        }

        [GridWidth(250)]
        [Group(Sections.Connection)]
        [DisplayOrder(100)]
        [DisplayName("Source Instance")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections), allowAddNew: false)]
        [ConnectionFor(nameof(Solution))]
        public SavedXrmRecordConfiguration SourceConnection { get; set; }

        [DoNotAllowAdd]
        [GridWidth(250)]
        [Group(Sections.Connection)]
        [DisplayOrder(100)]
        [DisplayName("Target Instance")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections), allowAddNew: false)]
        public SavedXrmRecordConfiguration TargetConnection { get; set; }

        [EditableFormWidth(175)]
        [PropertyInContextByPropertyNotNull(nameof(SourceConnection))]
        [DisplayOrder(510)]
        [RequiredProperty]
        [Group(Sections.Solution)]
        [GridWidth(150)]
        [ReferencedType(Entities.solution)]
        [LookupCondition(Fields.solution_.ismanaged, false)]
        [LookupCondition(Fields.solution_.isvisible, true)]
        [LookupFieldCascade(nameof(SourceVersionForRelease), Fields.solution_.version)]
        [LookupFieldCascade(nameof(Version), Fields.solution_.version)]
        [LookupFieldCascade(nameof(UniqueName), Fields.solution_.uniquename)]
        [LookupFieldCascade(nameof(FriendlyName), Fields.solution_.friendlyname)]
        [LookupFieldCascade(nameof(IsManaged), Fields.solution_.ismanaged)]
        [UsePicklist(Fields.solution_.uniquename)]
        public Lookup Solution { get; set; }

        [VersionPropertyValidator]
        [EditableFormWidth(60)]
        [GridWidth(110)]
        [Group(Sections.ExportDetails)]
        [PropertyInContextByPropertyNotNull(nameof(Solution))]
        [CascadeOnChange(nameof(SetSourceVersionPostRelease))]
        [DisplayOrder(520)]
        [RequiredProperty]
        public string SourceVersionForRelease { get; set; }

        [VersionPropertyValidator]
        [EditableFormWidth(60)]
        [GridWidth(110)]
        [Group(Sections.ExportDetails)]
        [PropertyInContextByPropertyNotNull(nameof(Solution))]
        [DisplayOrder(530)]
        [RequiredProperty]
        public string SetSourceVersionPostRelease { get; set; }

        [Hidden]
        public bool? IsManaged { get; set; }

        [Hidden]
        public string Version { get; set; }

        [Hidden]
        public string UniqueName { get; set; }

        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(200)]
        [ReadOnlyWhenSet]
        [Hidden]
        public string FriendlyName { get; set; }

        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(210)]
        [ReadOnlyWhenSet]
        [PropertyInContextByPropertyNotNull(nameof(TargetConnection))]
        [PropertyInContextByPropertyNotNull(nameof(UniqueName))]
        [PropertyInContextByPropertyValue(nameof(IsCurrentlyInstalledInTarget), false)]
        public bool? IsCurrentlyInstalledInTarget { get; set; }

        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(220)]
        [ReadOnlyWhenSet]
        [PropertyInContextByPropertyValue(nameof(IsCurrentlyInstalledInTarget), true)]
        public string CurrentTargetVersion { get; set; }


        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(230)]
        [ReadOnlyWhenSet]
        [PropertyInContextByPropertyValue(nameof(InstallAsManaged), true)]
        [PropertyInContextByPropertyValue(nameof(IsCurrentlyInstalledInTarget), true)]
        [PropertyInContextByPropertyValue(nameof(CurrentTargetVersionManaged), false)]
        [RequiredPropertyValue(true, "A managed solution cannot be installed into an instance where that solution is already unmanaged. Delete the solution from the target instance and try again")]
        public bool? CurrentTargetVersionManaged { get; set; }

        [Hidden]
        public bool IsInstallingNewerVersion
        {
            get
            {
                return !string.IsNullOrWhiteSpace(CurrentTargetVersion) && !string.IsNullOrWhiteSpace(SourceVersionForRelease) && VersionHelper.IsNewerVersion(SourceVersionForRelease, CurrentTargetVersion);
            }
        }

        [PropertyInContextByPropertyNotNull(nameof(Solution))]
        [PropertyInContextByPropertyNotNull(nameof(SourceConnection))]
        [PropertyInContextByPropertyNotNull(nameof(TargetConnection))]
        [GridWidth(110)]
        [Group(Sections.InstallOptions)]
        [DisplayOrder(300)]
        public bool InstallAsManaged { get; set; }

        [Group(Sections.InstallOptions)]
        [DisplayOrder(310)]
        [PropertyInContextByPropertyNotNull(nameof(Solution))]
        [PropertyInContextByPropertyNotNull(nameof(SourceConnection))]
        [PropertyInContextByPropertyNotNull(nameof(TargetConnection))]
        public bool OverwriteCustomisations { get; set; }

        [Group(Sections.InstallOptions)]
        [DisplayOrder(320)]
        [PropertyInContextByPropertyValue(nameof(IsInstallingNewerVersion), true)]
        [PropertyInContextByPropertyValue(nameof(CurrentTargetVersionManaged), true)]
        [PropertyInContextByPropertyValue(nameof(InstallAsManaged), true)]
        [PropertyInContextByPropertyNotNull(nameof(SourceConnection))]
        [PropertyInContextByPropertyNotNull(nameof(TargetConnection))]
        public bool InstallAsUpgrade { get; set; }

        private byte[] solutionZipContent;

        public void SetSolutionZipContent(byte[] byteArray)
        {
            solutionZipContent = byteArray;
        }

        byte[] IImportSolutionsRequestItem.GetSolutionZipContent()
        {
            return solutionZipContent;
        }

        private static class Sections
        {
            public const string Solution = "Main";
            public const string Connection = "Connection";
            public const string ExportDetails = "Export Details";
            public const string CurrentSolutionDetails = "Current Solution Details";
            public const string InstallOptions = "Install Options";
        }
    }
}