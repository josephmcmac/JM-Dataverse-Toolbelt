﻿using System;
using JosephM.Application.ViewModel.RecordEntry.Form;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class DateFieldViewModel : FieldViewModel<DateTime?>
    {
        public DateFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {

        }

        public string Format
        {
            get
            {
                return IncludeTime
                    ? "dd/MM/yyyy hh:mm:ss tt"
                    : "dd/MM/yyyy";
            }
        }

        public bool IncludeTime { get; set; }

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