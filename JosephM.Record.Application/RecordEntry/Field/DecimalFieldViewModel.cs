#region

using System;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Core.Extentions;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class DecimalFieldViewModel : FieldViewModel<decimal?>
    {
        public DecimalFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
            MinValue = Decimal.MinValue;
            MaxValue = Decimal.MaxValue;
        }

        public decimal MaxValue { get; set; }
        public decimal MinValue { get; set; }

        public string ValueString
        {
            get
            {
                if (ValueObject == null)
                    return null;
                return IsEditable
                    ? Value.Value.ToString()
                    : Value.Value.ToString(string.Format("0.{0}", "#".ReplicateString(RecordEntryViewModel.RecordService.GetFieldMetadata(FieldName, GetRecordType()).DecimalPrecision)));
            }
            set
            {
                if (value == null)
                    ValueObject = null;
                else
                    ValueObject = decimal.Parse(value);
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
                    response.AddInvalidReason(
                        string.Format("The entered value is greater than the maximum of {0}", MaxValue));
                }
                if (decimalValue < MinValue)
                {
                    response.AddInvalidReason(
                        string.Format("The entered value is less than the minimum of {0}", MinValue));
                }
            }
            else if (IsNotNullable && (value == null || string.IsNullOrWhiteSpace(value.ToString())))
            {
                response.AddInvalidReason(string.Format("A Value Is Required"));
            }
            return response;
        }
    }
}