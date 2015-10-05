#region

using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class RecordTypeFieldViewModel : DropdownFieldViewModel<RecordType>
    {
        public RecordTypeFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public override RecordType Value
        {
            get { return ValueObject as RecordType; }
            set
            {
                ValueObject = value;
                OnPropertyChanged("Value");
            }
        }
    }
}