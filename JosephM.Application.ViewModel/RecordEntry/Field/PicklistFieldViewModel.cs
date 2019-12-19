using System;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class PicklistFieldViewModel : DropdownFieldViewModel<PicklistOption>
    {
        public PicklistFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public override PicklistOption Value
        {
            get
            {
                if (ValueObject is Enum item)
                {
                    return PicklistOption.EnumToPicklistOption(item);
                }
                return ValueObject as PicklistOption;
            }
            set { ValueObject = value; }
        }
    }
}