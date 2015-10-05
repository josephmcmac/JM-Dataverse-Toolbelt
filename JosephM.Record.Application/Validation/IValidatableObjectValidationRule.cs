#region

using System;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Core.Attributes;
using JosephM.Core.Service;

#endregion

namespace JosephM.Application.ViewModel.Validation
{
    /// <summary>
    ///     Encapsulates A Custom Validation Rule Derived From JosephM.Core.Attributes.ValidationRule In The Type Required By
    ///     The .NET Runtime
    /// </summary>
    public class IValidatableObjectValidationRule : ValidationRuleBase
    {
        protected override IsValidResponse ValidateExtention()
        {
            var response = new IsValidResponse();
           if(!(ChangedField is FieldViewModelBase))
               throw new NotImplementedException(string.Format("This Form Of Validation Only Supported For {0} Objects", typeof(FieldViewModelBase).Name));
            var fieldViewModel = (FieldViewModelBase) ChangedField;
            var value = fieldViewModel.ValueObject;
            if (value != null)
            {
                if (!(value is IValidatableObject))
                    throw new NotImplementedException(string.Format("Error Object Does Not Implement {0}", typeof(IValidatableObject).Name));
                response = ((IValidatableObject)value).Validate();
            }
            return response;
        }
    }
}