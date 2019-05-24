#region

using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Record.Service;
using JosephM.Core.Extentions;
using JosephM.Record.Extentions;

#endregion

namespace JosephM.Application.ViewModel.Validation
{
    /// <summary>
    ///     Encapsulates A Custom Validation Rule Derived From JosephM.Core.Attributes.ValidationRule In The Type Required By
    ///     The .NET Runtime
    /// </summary>
    public class PropertyAttributeValidationRule : ValidationRuleBase
    {
        public PropertyAttributeValidationRule(PropertyValidator propertyValidator)
        {
            PropertyValidator = propertyValidator;
        }

        private PropertyValidator PropertyValidator { get; set; }

        protected override IsValidResponse ValidateExtention()
        {
            var record = RecordForm.GetRecord();
            var response = new IsValidResponse();

            if (!PropertyValidator.IsValid(record.GetField(ChangedField.ReferenceName)))
            {
                response.AddInvalidReason(PropertyValidator.GetErrorMessage(RecordForm.RecordService.GetFieldMetadata(ChangedField.ReferenceName, RecordForm.GetRecordType()).DisplayName));
            }

            return response;
        }
    }
}