#region

using System;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Record.Application.RecordEntry.Field
{
    public class IntegerFieldViewModel : FieldViewModel<int?>
    {
        public IntegerFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
            MinValue = Int32.MinValue;
            MaxValue = Int32.MaxValue;
        }


        public int MaxValue { get; set; }
        public int MinValue { get; set; }

        protected override IsValidResponse VerifyValueRequest(object value)
        {
            var response = new IsValidResponse();
            if (value != null && !string.IsNullOrWhiteSpace(value.ToString()))
            {
                var intValue = int.Parse(value.ToString());
                if (intValue > MaxValue)
                {
                    response.AddInvalidReason(
                        string.Format("The entered value is greater than the maximum of {0}", MaxValue));
                }
                if (intValue < MinValue)
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