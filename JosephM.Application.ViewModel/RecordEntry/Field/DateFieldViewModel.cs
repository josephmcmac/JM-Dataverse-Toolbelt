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

        public override string StringDisplay { get { return Value?.ToString(Format); } }

        public override DateTime? Value
        {
            get
            {
                if (!base.Value.HasValue)
                    return null;
                if (base.Value.Value.Kind == DateTimeKind.Utc)
                    return base.Value?.ToLocalTime();
                return base.Value;
            }
            set
            {
                if (!value.HasValue)
                    base.Value = null;
                else if (value.Value.Kind == DateTimeKind.Local)
                    base.Value = value.Value.ToUniversalTime();
                else
                    base.Value = value;
            }
        }
    }
}