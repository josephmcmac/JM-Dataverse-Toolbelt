using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
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
    public class SolutionTransferRequest : ServiceRequestBase
    {
        [GridWidth(250)]
        [Group(Sections.Connection)]
        [DisplayOrder(100)]
        [DisplayName("Source Instance")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
        [ConnectionFor(nameof(Solution))]
        public SavedXrmRecordConfiguration SourceConnection { get; set; }

        [GridWidth(250)]
        [Group(Sections.Connection)]
        [DisplayOrder(100)]
        [DisplayName("Target Instance")]
        [RequiredProperty]
        [SettingsLookup(typeof(ISavedXrmConnections), nameof(ISavedXrmConnections.Connections))]
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
        [UsePicklist(Fields.solution_.uniquename)]
        public Lookup Solution { get; set; }

        [PropertyInContextByPropertyNotNull(nameof(Solution))]
        [GridWidth(110)]
        [Group(Sections.ExportDetails)]
        [DisplayOrder(500)]
        public bool ExportAsManaged { get; set; }

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

        private static class Sections
        {
            public const string Solution = "Main";
            public const string Connection = "Connection";
            public const string ExportDetails = "Export Details";
        }
    }
}