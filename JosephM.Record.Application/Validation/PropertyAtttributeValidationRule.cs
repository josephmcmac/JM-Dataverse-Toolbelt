#region

using JosephM.Core.Attributes;
using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.Service;

#endregion

namespace JosephM.Record.Application.Validation
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
            var instance = ((ObjectRecord) RecordForm.GetRecord()).Instance;
            var response = new IsValidResponse();

            if (!PropertyValidator.IsValid(instance.GetPropertyValue(ChangedField.ReferenceName), instance))
                response.AddInvalidReason(PropertyValidator.GetErrorMessage(ChangedField.ReferenceName, instance));

            return response;
        }
    }
}