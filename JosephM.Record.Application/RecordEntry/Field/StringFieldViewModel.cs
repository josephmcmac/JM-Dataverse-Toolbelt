using JosephM.Record.Application.RecordEntry.Form;

namespace JosephM.Record.Application.RecordEntry.Field
{
    public class StringFieldViewModel : FieldViewModel<string>
    {
        public StringFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public override string Value
        {
            get { return ValueObject == null ? null : ValueObject.ToString(); }
            set { ValueObject = value; }
        }

        public int? MaxLength { get; set; }
    }
}