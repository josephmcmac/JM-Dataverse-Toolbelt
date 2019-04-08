using JosephM.Application.ViewModel.RecordEntry.Form;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public abstract class FieldViewModel<T> : FieldViewModelBase
    {
        protected FieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public virtual T Value
        {
            get { return (T)ValueObject; }
            set { ValueObject = value; }
        }

        public virtual string StringDisplay
        {
            get
            {
                return Value?.ToString();
            }
        }

        public override void CallOnPropertyChangeEvents()
        {
            OnPropertyChanged(nameof(ValueObject));
            OnPropertyChanged(nameof(StringDisplay));
            OnPropertyChanged(nameof(Value));
        }
    }
}