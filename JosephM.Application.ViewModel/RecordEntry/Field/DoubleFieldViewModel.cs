#region

using System;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Service;
using JosephM.Core.Extentions;
using JosephM.Record.Extentions;
using System.Globalization;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class DoubleFieldViewModel : FieldViewModel<double?>
    {
        public DoubleFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
            MinValue = double.MinValue;
            MaxValue = double.MaxValue;
        }

        public double MaxValue { get; set; }
        public double MinValue { get; set; }

        private int DecimalPrecision
        {
            get
            {
                return RecordEntryViewModel.RecordService.GetDecimalPrecision(FieldName, GetRecordType());
            }
        }

        private NumberFormatInfo _numberFormatInfo;
        private NumberFormatInfo NumberFormatInfo
        {
            get
            {
                if (_numberFormatInfo == null)
                {
                    _numberFormatInfo = (NumberFormatInfo)RecordEntryViewModel.RecordService.GetLocalisationService().NumberFormatInfo.Clone();
                    _numberFormatInfo.NumberDecimalDigits = DecimalPrecision;
                }
                return _numberFormatInfo;
            }
        }

        public string ValueString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ValueObject?.ToString()))
                    return null;
                return IsEditable
                    ? Value.Value.ToString($"n{DecimalPrecision}", NumberFormatInfo)
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
                    double updatedDouble = 0;
                    if (double.TryParse(value, NumberStyles.Any, NumberFormatInfo, out updatedDouble))
                    {
                        ValueObject = updatedDouble;
                        OnPropertyChanged(nameof(ValueString));
                    }
                    else
                    {
                        ApplicationController.UserMessage($"{value} could not be parsed to {nameof(Double)}");
                        OnPropertyChanged(nameof(ValueString));
                    }
                }
            }
        }

        protected override IsValidResponse VerifyValueRequest(object value)
        {
            var response = new IsValidResponse();
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                var doubleValue = double.Parse(value.ToString());
                if (doubleValue > MaxValue)
                {
                    response.AddInvalidReason($"The entered value is greater than the maximum of {MaxValue}");
                }
                if (doubleValue < MinValue)
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
    }
}