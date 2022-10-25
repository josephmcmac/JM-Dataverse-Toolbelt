﻿using JosephM.Core.Attributes;
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
    [Group(Sections.Solution, Group.DisplayLayoutEnum.VerticalCentered, 20, displayLabel: false)]
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

        [PropertyInContextByPropertyNotNull(nameof(SourceConnection))]
        [DisplayOrder(510)]
        [RequiredProperty]
        [Group(Sections.Solution)]
        [GridWidth(150)]
        [ReferencedType(Entities.solution)]
        [LookupCondition(Fields.solution_.ismanaged, false)]
        [LookupCondition(Fields.solution_.isvisible, true)]
        [LookupFieldCascade(nameof(ThisReleaseVersion), Fields.solution_.version)]
        [LookupFieldCascade(nameof(UniqueName), Fields.solution_.uniquename)]
        [LookupFieldCascade(nameof(FriendlyName), Fields.solution_.friendlyname)]
        [UsePicklist(Fields.solution_.uniquename)]
        public Lookup Solution { get; set; }

        [GridWidth(110)]
        [Group(Sections.ExportDetails)]
        [PropertyInContextByPropertyNotNull(nameof(Solution))]
        [CascadeOnChange(nameof(SetVersionPostRelease))]
        [DisplayOrder(520)]
        [RequiredProperty]
        public string ThisReleaseVersion { get; set; }

        [GridWidth(110)]
        [Group(Sections.ExportDetails)]
        [PropertyInContextByPropertyNotNull(nameof(Solution))]
        [DisplayOrder(530)]
        [RequiredProperty]
        public string SetVersionPostRelease { get; set; }

        [Hidden]
        public bool? IsManaged { get; set; }

        [Hidden]
        public string Version { get; set; }

        [Hidden]
        public string UniqueName { get; set; }

        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(200)]
        [ReadOnlyWhenSet]
        [PropertyInContextByPropertyNotNull(nameof(FriendlyName))]
        [DisplayName("Solution Name")]
        public string FriendlyName { get; set; }

        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(210)]
        [ReadOnlyWhenSet]
        [PropertyInContextByPropertyNotNull(nameof(TargetConnection))]
        [PropertyInContextByPropertyNotNull(nameof(UniqueName))]
        [PropertyInContextByPropertyValue(nameof(IsCurrentlyInstalled), false)]
        public bool? IsCurrentlyInstalled { get; set; }

        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(220)]
        [ReadOnlyWhenSet]
        [PropertyInContextByPropertyValue(nameof(IsCurrentlyInstalled), true)]
        public string CurrentVersion { get; set; }


        [Group(Sections.CurrentSolutionDetails)]
        [DisplayOrder(230)]
        [ReadOnlyWhenSet]
        [PropertyInContextByPropertyValue(nameof(InstallAsManaged), true)]
        [PropertyInContextByPropertyValue(nameof(IsCurrentlyInstalled), true)]
        [PropertyInContextByPropertyValue(nameof(CurrentVersionManaged), false)]
        [RequiredPropertyValue(true, "A managed solution cannot be installed into an instance where that solution is already unmanaged. Delete the solution from the target instance and try again")]
        public bool? CurrentVersionManaged { get; set; }

        [Hidden]
        public bool CurrentIsEarlierVersion
        {
            get
            {
                return !string.IsNullOrWhiteSpace(CurrentVersion) && !string.IsNullOrWhiteSpace(ThisReleaseVersion) && VersionHelper.IsNewerVersion(ThisReleaseVersion, CurrentVersion);
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
        [PropertyInContextByPropertyValue(nameof(CurrentIsEarlierVersion), true)]
        [PropertyInContextByPropertyValue(nameof(CurrentVersionManaged), true)]
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