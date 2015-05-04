using JosephM.Record.Application.RecordEntry.Form;

namespace JosephM.Record.Application.RecordEntry.Field
{
    public abstract class FieldViewModel<T> : FieldViewModelBase
    {
        protected FieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public virtual T Value
        {
            get { return (T) ValueObject; }
            set { ValueObject = value; }
        }
    }
}