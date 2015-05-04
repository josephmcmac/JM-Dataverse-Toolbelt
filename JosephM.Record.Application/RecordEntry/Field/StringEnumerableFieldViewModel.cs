using System.Collections.Generic;
using JosephM.Record.Application.RecordEntry.Form;

namespace JosephM.Record.Application.RecordEntry.Field
{
    public class StringEnumerableFieldViewModel : FieldViewModel<IEnumerable<string>>
    {
        public StringEnumerableFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }

        public int? MaxLength { get; set; }
    }
}