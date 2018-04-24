#region

using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.FieldType;
using JosephM.Core.Service;

#endregion

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class IntegerFieldViewModel : FieldViewModel<int?>
    {
        public IntegerFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm, IEnumerable<PicklistOption> picklist)
            : base(fieldName, label, recordForm)
        {
            MinValue = Int32.MinValue;
            MaxValue = Int32.MaxValue;
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
                if (Value.HasValue && PicklistOptions.Any(p => p.Key == Value.Value.ToString()))
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