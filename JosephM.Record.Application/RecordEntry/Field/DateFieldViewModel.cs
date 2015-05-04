#region

using System;
using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Record.Application.RecordEntry.Field
{
    public class DateFieldViewModel : FieldViewModel<DateTime?>
    {
        public DateFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
        }
    }
}