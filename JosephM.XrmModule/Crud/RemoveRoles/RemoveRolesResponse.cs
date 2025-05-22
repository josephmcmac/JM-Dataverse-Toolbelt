using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.ComponentModel;

namespace JosephM.XrmModule.Crud.RemoveRoles
{
    [Group(Sections.Counts, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 0, displayLabel: false)]
    public class RemoveRolesResponse : ServiceResponseBase<RemoveRolesResponseItem>, INotifyPropertyChanged
    {
        private int _countRoleRemoved;
        private int _countRoleNotPresent;
        private int _numberOfErrors;

        [DisplayOrder(50)]
        [Group(Sections.Counts)]
        [Core.Attributes.DisplayName("Count Role Removed")]
        public int CountRoleRemoved
        {
            get { return _countRoleRemoved; }
            set
            {
                _countRoleRemoved = value;
                OnPropertyChanged(nameof(CountRoleRemoved));
            }
        }

        [DisplayOrder(60)]
        [Group(Sections.Counts)]
        [Core.Attributes.DisplayName("Count Role Not Present")]
        public int CountRoleNotPresent
        {
            get { return _countRoleNotPresent; }
            set
            {
                _countRoleNotPresent = value;
                OnPropertyChanged(nameof(CountRoleNotPresent));
            }
        }

        [DisplayOrder(70)]
        [Group(Sections.Counts)]
        [Core.Attributes.DisplayName("Number of Errors")]
        public int NumberOfErrors
        {
            get { return _numberOfErrors; }
            set
            {
                _numberOfErrors = value;
                OnPropertyChanged(nameof(NumberOfErrors));
            }
        }

        private static class Sections
        {
            public const string Counts = "Counts";
        }
    }
}