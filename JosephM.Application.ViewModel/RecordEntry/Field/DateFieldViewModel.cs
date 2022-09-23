using System;
using System.Collections.Generic;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;

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
                    ? RecordEntryViewModel.RecordService.GetLocalisationService().DateTimeFormatString
                    : RecordEntryViewModel.RecordService.GetLocalisationService().DateFormatString;
            }
        }


        public bool IncludeTime { get; set; }

        public override string StringDisplay { get { return Value?.ToString(Format); } }

        public override DateTime? Value
        {
            get
            {
                if (!base.Value.HasValue)
                {
                    return null;
                }
                if (base.Value.Value.Kind == DateTimeKind.Utc)
                {
                    return RecordEntryViewModel.RecordService.GetLocalisationService().ConvertUtcToLocalTime(base.Value.Value);
                }
                return base.Value;
            }
            set
            {
                if (!value.HasValue)
                {
                    base.Value = null;
                }
                else if (value.Value.Kind == DateTimeKind.Local)
                {
                    base.Value = value.Value.ToUniversalTime();
                }
                else
                {
                    base.Value = value;
                }
            }
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get
            {
                if(_selectedDate == null && Value.HasValue)
                {
                    _selectedDate = Value.Value;
                }
                return _selectedDate;
            }
            set
            {
                UnloadSelectionsToDateValue(value, _selectedHour, _selectedMinute, _selectedAmPm);
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
            }
        }

        private void UnloadSelectionsToDateValue(DateTime? selectedDate, PicklistOption selectedHour, PicklistOption selectedMinute, PicklistOption selectedAmPm)
        {
            if (selectedDate.HasValue)
            {
                var dateValue = selectedDate.Value;
                if (selectedHour == null)
                {
                    selectedHour = new PicklistOption("12", "12");
                }
                var hourInt = int.Parse(selectedHour.Key);
                if (selectedMinute == null)
                {
                    selectedMinute = new PicklistOption("0", "00");
                }
                var minuteInt = int.Parse(selectedMinute.Key);
                if (selectedAmPm == null)
                {
                    selectedAmPm = new PicklistOption("AM", "AM");
                }
                var minuteAmPm = selectedAmPm.Key;
                if (minuteAmPm == "AM" && hourInt == 12)
                {
                    hourInt = 0;
                }
                if (minuteAmPm == "PM" && hourInt < 12)
                {
                    hourInt += 12;
                }

                Value = new DateTime(dateValue.Year, dateValue.Month, dateValue.Day, hourInt, minuteInt, dateValue.Second, dateValue.Kind);

                if (Value.HasValue)
                {
                    if (_selectedHour == null)
                    {
                        _selectedHour = selectedHour;
                        OnPropertyChanged(nameof(SelectedHour));
                    }
                    if (_selectedMinute == null)
                    {
                        _selectedMinute = selectedMinute;
                        OnPropertyChanged(nameof(selectedMinute));
                    }
                    if (_selectedAmPm == null)
                    {
                        _selectedAmPm = selectedAmPm;
                        OnPropertyChanged(nameof(SelectedAmPm));
                    }
                }
            }
            else
            {
                Value = null;
                if (Value == null)
                {
                    _selectedHour = null;
                    OnPropertyChanged(nameof(SelectedHour));
                    _selectedMinute = null;
                    OnPropertyChanged(nameof(SelectedMinute));
                    _selectedAmPm = null;
                    OnPropertyChanged(nameof(SelectedAmPm));
                }
            }
        }

        public PicklistOption _selectedHour;
        public PicklistOption SelectedHour
        {
            get
            {
                if (_selectedHour == null && Value.HasValue)
                {
                    var hour = Value.Value.Hour == 0
                        ? 12
                        : Value.Value.Hour > 12
                        ? Value.Value.Hour - 12
                        : Value.Value.Hour;
                    _selectedHour = new PicklistOption(hour.ToString(), hour.ToString("00"));
                }
                return _selectedHour;
            }
            set
            {
                UnloadSelectionsToDateValue(_selectedDate, value, _selectedMinute, _selectedAmPm);
                _selectedHour = value;
                OnPropertyChanged(nameof(SelectedHour));
            }
        }

        public PicklistOption _selectedMinute;
        public PicklistOption SelectedMinute
        {
            get
            {
                if (_selectedMinute == null && Value.HasValue)
                {
                    var minute = Value.Value.Minute;
                    _selectedMinute = new PicklistOption(minute.ToString(), minute.ToString("00"));
                }
                return _selectedMinute;
            }
            set
            {
                UnloadSelectionsToDateValue(_selectedDate, _selectedHour, value, _selectedAmPm);
                _selectedMinute = value;
                OnPropertyChanged(nameof(SelectedMinute));
            }
        }

        public PicklistOption _selectedAmPm;
        public PicklistOption SelectedAmPm
        {
            get
            {
                if(_selectedAmPm == null && Value.HasValue)
                {
                    var amPm = Value.Value.Hour > 11 ? "PM" : "AM";
                    _selectedAmPm = new PicklistOption(amPm, amPm);
                }
                return _selectedAmPm;
            }
            set
            {
                UnloadSelectionsToDateValue(_selectedDate, _selectedHour, _selectedMinute, value);
                _selectedAmPm = value;
                OnPropertyChanged(nameof(SelectedAmPm));
            }
        }

        public IEnumerable<PicklistOption> HourOptions
        {
            get
            {
                for(var i = 1; i <= 12; i++)
                {
                    yield return new PicklistOption(i.ToString(), i.ToString("00"));
                }
            }
        }

        public IEnumerable<PicklistOption> MinuteOptions
        {
            get
            {
                for (var i = 0; i < 60; i++)
                {
                    yield return new PicklistOption(i.ToString(), i.ToString("00"));
                }
            }
        }

        public IEnumerable<PicklistOption> AmPmOptions
        {
            get
            {
                return new[]
                {
                    new PicklistOption("AM", "AM"),
                    new PicklistOption("PM", "PM")
                };
            }
        }
    }
}