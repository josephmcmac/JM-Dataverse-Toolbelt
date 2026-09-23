using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Xrm.Schema;

namespace JosephM.UserSavedObjectsUtility.Views
{
    [DisplayName("Saved Views Utility")]
    [Instruction("Export, clone, and share, user saved personal views")]
    [Group(Sections.UserOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, displayLabel: false)]
    [Group(Sections.TableOptions, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 20, displayLabel: false)]
    public class UserSavedViewsUtilityRequest : ServiceRequestBase
    {
        public UserSavedViewsUtilityRequest()
        {
            IncludeAllOwningUsers = true;
            IncludeViewsForAllTables = true;
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
        [DisplayName("Include views for all or just one specific table")]
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