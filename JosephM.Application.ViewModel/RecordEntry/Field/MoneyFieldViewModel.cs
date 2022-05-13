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
    public class MoneyFieldViewModel : FieldViewModel<decimal?>
    {
        public MoneyFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
            MinValue = decimal.MinValue;
            MaxValue = decimal.MaxValue;
        }

        public decimal MaxValue { get; set; }
        public decimal MinValue { get; set; }

        private int DecimalPrecision
        {
            get
            {
                return RecordEntryViewModel.RecordService.GetCurrencyPrecision(CurrencyId);
            }
        }

        private string CurrencyId
        {
            get
            {
                return RecordEntryViewModel.RecordService.GetCurrencyId(Record, FieldName); 
            }
        }

        private NumberFormatInfo _numberFormatInfo;
        private NumberFormatInfo NumberFormatInfo
        {
            get
            {
                if(_numberFormatInfo == null)
                {
                    _numberFormatInfo = (NumberFormatInfo)RecordEntryViewModel.RecordService.GetLocalisationService().NumberFormatInfo.Clone();
                    _numberFormatInfo.NumberDecimalDigits = DecimalPrecision;
                }
                return _numberFormatInfo;
            }
        }

        public override string StringDisplay
        {
            get
            {
                return RecordEntryViewModel.RecordService.GetFieldAsDisplayString(GetRecordTypeOfThisField
                    (), IndexFieldName, ValueObject, currencyId: CurrencyId);
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
                    ? Value.Value.ToString($"n{DecimalPrecision}", NumberFormatInfo)
                    : RecordEntryViewModel.RecordService.GetFieldAsDisplayString(Record, FieldName, currencyId: CurrencyId);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    ValueObject = null;
                }
                else
                {
                    decimal updatedDecimal = 0;
                    if (decimal.TryParse(value, NumberStyles.Any, NumberFormatInfo, out updatedDecimal))
                    {
                        ValueObject = updatedDecimal;
                        OnPropertyChanged(nameof(ValueString));
                    }
                    else
                    {
                        ApplicationController.UserMessage($"{value} could not be parsed to {nameof(Decimal)}");
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
                var decimalValue = decimal.Parse(value.ToString());
                if (decimalValue > MaxValue)
                {
                    response.AddInvalidReason($"The entered value is greater than the maximum of {MaxValue}");
                }
                if (decimalValue < MinValue)
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