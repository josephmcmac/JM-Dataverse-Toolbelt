using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class UrlFieldViewModel : FieldViewModel<Url>
    {
        public UrlFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public override Url Value
        {
            get { return ValueObject as Url; }
            set { ValueObject = value; }
        }

        public string LinkName { get { return Value?.Label; } }

        public string LinkUri { get { return Value?.Uri; } }
    }
}