using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.ComponentModel;

namespace JosephM.XrmModule.Crud.AddRoles
{
    [Group(Sections.Counts, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 0, displayLabel: false)]
    public class AddRolesResponse : ServiceResponseBase<AddRolesResponseItem>, INotifyPropertyChanged
    {
        private int _countRoleAdded;
        private int _countRoleAlreadyPresent;
        private int _numberOfErrors;

        [DisplayOrder(50)]
        [Group(Sections.Counts)]
        [Core.Attributes.DisplayName("Count Role Added")]
        public int CountRoleAdded
        {
            get { return _countRoleAdded; }
            set
            {
                _countRoleAdded = value;
                OnPropertyChanged(nameof(CountRoleAdded));
            }
        }

        [DisplayOrder(60)]
        [Group(Sections.Counts)]
        [Core.Attributes.DisplayName("Count Role Already Present")]
        public int CountRoleAlreadyPresent
        {
            get { return _countRoleAlreadyPresent; }
            set
            {
                _countRoleAlreadyPresent = value;
                OnPropertyChanged(nameof(CountRoleAlreadyPresent));
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