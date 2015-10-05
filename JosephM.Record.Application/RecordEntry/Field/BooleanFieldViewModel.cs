using JosephM.Application.ViewModel.RecordEntry.Form;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class BooleanFieldViewModel : FieldViewModel<bool>
    {
        public BooleanFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public override bool Value
        {
            get { return ValueObject != null && (bool)ValueObject; }
            set { ValueObject = value; }
        }
    }
}