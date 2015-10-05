#region

using System;
using JosephM.Application.ViewModel.RecordEntry.Form;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class DateFieldViewModel : FieldViewModel<DateTime?>
    {
        public DateFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
            Format = "dd/MM/yyyy hh:mm:ss tt";
        }

        public string Format { get; set; }
    }
}