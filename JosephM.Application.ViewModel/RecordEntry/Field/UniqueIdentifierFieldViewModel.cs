using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Service;
using System;

namespace JosephM.Application.ViewModel.RecordEntry.Field
{
    public class UniqueIdentifierFieldViewModel : FieldViewModel<string>
    {
        public UniqueIdentifierFieldViewModel(string fieldName, string label, RecordEntryViewModelBase recordForm)
            : base(fieldName, label, recordForm)
        {
            EditableFormWidth = 245;
        }

        protected override IsValidResponse VerifyValueRequest(object value)
        {
            var response = new IsValidResponse();
            if (value != null && !(value is Guid) && !string.IsNullOrEmpty(value.ToString()))
            {
                Guid dummy = Guid.Empty;
                if (!Guid.TryParse(value.ToString(), out dummy))
                    response.AddInvalidReason("Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)");
            }
            return response;
        }
    }
}