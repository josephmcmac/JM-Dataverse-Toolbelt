using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;

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
            get
            {
                return ValueObject is string
                    ? new RecordType(ValueObject.ToString(), ValueObject.ToString())
                    : ValueObject as RecordType;
            }
            set
            {
                ValueObject = value;
                OnPropertyChanged(nameof(Value));
            }
        }
    }
}