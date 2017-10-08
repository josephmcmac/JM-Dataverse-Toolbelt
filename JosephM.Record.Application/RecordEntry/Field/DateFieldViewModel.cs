#region

using System;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Extentions;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class DateFieldViewModel : FieldViewModel<DateTime?>
    {
        public DateFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
            Format = "dd/MM/yyyy hh:mm:ss tt";
            if (IsReadOnly && !recordForm.RecordService.GetFieldMetadata(FieldName, GetRecordType()).IncludeTime)
                Format = "dd/MM/yyyy";
        }

        public string Format { get; set; }

        public string FormattedDate { get { return Value?.ToString(Format); } }

        public override DateTime? Value
        {
            get
            {
                return base.Value?.ToLocalTime();
            }
            set
            {
                base.Value = value?.ToUniversalTime();
            }
        }
    }
}