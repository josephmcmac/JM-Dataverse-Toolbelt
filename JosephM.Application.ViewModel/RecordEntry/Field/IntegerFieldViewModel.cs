using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class IntegerFieldViewModel : FieldViewModel<int?>
    {
        public IntegerFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm, IEnumerable<PicklistOption> picklist)
            : base(fieldName, label, recordForm)
        {
            MinValue = int.MinValue;
            MaxValue = int.MaxValue;
            PicklistOptions = picklist;
            UsePicklist = PicklistOptions != null && PicklistOptions.Any();
        }

        public int MaxValue { get; set; }

        public bool UsePicklist { get; set; }
        public IEnumerable<PicklistOption> PicklistOptions { get; set; }

        public PicklistOption SelectedOption
        {
            get
            {
                if (Value.HasValue && PicklistOptions != null && PicklistOptions.Any(p => p.Key == Value.Value.ToString()))
                    return PicklistOptions.First(p => p.Key == Value.Value.ToString());
                return null;
            }
            set
            {
                if (value != null)
                {
                    try
                    {
                        Value = int.Parse(value.Key);
                    }
                    catch (Exception ex)
                    {
                        ApplicationController.ThrowException(ex);
                    }
                }
                else
                    Value = null;
                OnPropertyChanged(nameof(SelectedOption));
            }
        }

        public int MinValue { get; set; }

        protected override IsValidResponse VerifyValueRequest(object value)
        {
            var response = new IsValidResponse();
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                //this needs to try parse and throw 
                var intValue = int.Parse(value.ToString());
                if (intValue > MaxValue)
                {
                    response.AddInvalidReason($"The entered value is greater than the maximum of {MaxValue}");
                }
                if (intValue < MinValue)
                {
                    response.AddInvalidReason($"The entered value is less than the minimum of {MinValue}");
                }
            }
            else if (IsNotNullable && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
            {
                response.AddInvalidReason("A Value Is Required");
            }
            return response;
        }

        private NumberFormatInfo NumberFormatInfo
        {
            get
            {
                return RecordEntryViewModel.RecordService.GetLocalisationService().NumberFormatInfo;
            }
        }

        public override void CallOnPropertyChangeEvents()
        {
            OnPropertyChanged(nameof(ValueString));
            base.CallOnPropertyChangeEvents();
        }

        public string ValueString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ValueObject?.ToString()))
                    return null;
                return IsEditable
                    ? Value.Value.ToString($"n0", NumberFormatInfo)
                    : RecordEntryViewModel.RecordService.GetFieldAsDisplayString(Record, FieldName);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    ValueObject = null;
                }
                else
                {
                    int updatedInteger = 0;
                    if (int.TryParse(value, NumberStyles.Any, NumberFormatInfo, out updatedInteger))
                    {
                        ValueObject = updatedInteger;
                        OnPropertyChanged(nameof(ValueString));
                    }
                    else
                    {
                        ApplicationController.UserMessage($"{value} could not be parsed to Integer");
                        OnPropertyChanged(nameof(ValueString));
                    }
                }
            }
        }

        public override string StringDisplay
        {
            get
            {
                if (!Value.HasValue)
                {
                    return null;
                }
                if (UsePicklist && SelectedOption != null)
                {
                    return SelectedOption.Value;
                }
                else
                {
                    return RecordEntryViewModel.RecordService.GetFieldAsDisplayString(Record, FieldName);
                }
            }
        }
    }
}