#region

using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using JosephM.Core.Service;
using JosephM.Record.Application.RecordEntry.Field;
using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Record.Application.Validation
{
    /// <summary>
    ///     Extends The Standard .NET Validation Rule For Validation
    /// </summary>
    public abstract class ValidationRuleBase : ValidationRule
    {
        protected ValidationRuleBase()
        {
            ValidationStep = ValidationStep.UpdatedValue;
        }

        protected RecordEntryViewModelBase RecordForm { get; private set; }
        protected IValidatable ChangedField { get; private set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var bindingExpression = value;
            if (bindingExpression == null)
                throw new ArgumentNullException("value");
            ChangedField = (IValidatable) ((BindingExpression) value).DataItem;
            RecordForm = ChangedField.GetRecordForm();
            var verifyResponse = ValidateExtention();
            return new ValidationResult(verifyResponse.IsValid, verifyResponse.GetErrorString());
        }

        public ValidationResult Validate(IValidatable fieldObject)
        {
            ChangedField = fieldObject;
            RecordForm = ChangedField.GetRecordForm();
            var verifyResponse = ValidateExtention();
            return new ValidationResult(verifyResponse.IsValid, verifyResponse.GetErrorString());
        }

        protected abstract IsValidResponse ValidateExtention();
    }
}