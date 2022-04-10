#region

using System;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Service;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class BigIntFieldViewModel : FieldViewModel<long?>
    {
        public BigIntFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
            MinValue = long.MinValue;
            MaxValue = long.MaxValue;
        }

        public long MaxValue { get; set; }
        public long MinValue { get; set; }

        protected override IsValidResponse VerifyValueRequest(object value)
        {
            var response = new IsValidResponse();
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                var intValue = long.Parse(value.ToString());
                if (intValue > MaxValue)
                {
                    response.AddInvalidReason(
                        $"The entered value is greater than the maximum of {MaxValue}");
                }
                if (intValue < MinValue)
                {
                    response.AddInvalidReason(
                        $"The entered value is less than the minimum of {MinValue}");
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