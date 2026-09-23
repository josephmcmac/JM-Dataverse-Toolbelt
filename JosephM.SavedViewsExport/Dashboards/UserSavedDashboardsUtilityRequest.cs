using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.Schema;

namespace JosephM.UserSavedObjectsUtility.Dashboards
{
    [DisplayName("User Saved Dashboard Utility")]
    [Instruction("Export, clone, and share, user saved personal dashboards")]
    [Group(Sections.UserOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, displayLabel: false)]
    public class UserSavedDashboardsUtilityRequest : ServiceRequestBase
    {
        public UserSavedDashboardsUtilityRequest()
        {
            IncludeAllOwningUsers = true;
        }

        [DisplayOrder(20)]
        [DisplayName("Include owned by all or just one specific user")]
        [Group(Sections.UserOptions)]
        [RequiredProperty]
        public bool IncludeAllOwningUsers { get; set; }

        [DisplayOrder(30)]
        [Group(Sections.UserOptions)]
        [PropertyInContextByPropertyValue(nameof(IncludeAllOwningUsers), false)]
        [ReferencedType(Entities.systemuser)]
        [UsePicklist]
        [RequiredProperty]
        public Lookup Owner { get; set; }

        private static class Sections
        {
            public const string UserOptions = "User Options";
        }
    }
}