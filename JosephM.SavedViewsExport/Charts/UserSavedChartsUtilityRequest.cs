using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.Schema;

namespace JosephM.UserSavedObjectsUtility.Charts
{
    [DisplayName("User Saved Chart Utility")]
    [Instruction("Export, clone, and share, user saved personal charts")]
    [Group(Sections.UserOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, displayLabel: false)]
    [Group(Sections.TableOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, displayLabel: false)]
    public class UserSavedChartsUtilityRequest : ServiceRequestBase
    {
        public UserSavedChartsUtilityRequest()
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

        [DisplayOrder(40)]
        [DisplayName("Include charts for all or just one specific table")]
        [Group(Sections.TableOptions)]
        [RequiredProperty]
        public bool IncludeViewsForAllTables { get; set; }

        [DisplayOrder(50)]
        [Group(Sections.TableOptions)]
        [PropertyInContextByPropertyValue(nameof(IncludeViewsForAllTables), false)]
        [RequiredProperty]
        public RecordType Table { get; set; }

        private static class Sections
        {
            public const string UserOptions = "User Options";
            public const string TableOptions = "Table Options";
        }
    }
}