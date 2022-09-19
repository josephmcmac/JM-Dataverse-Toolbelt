using JosephM.Core.Attributes;
using System;
using System.Linq;

namespace JosephM.XrmModule.Crud.Validations
{
    /// <summary>
    /// Validates publiher prefix field
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class PrefixValidation : PropertyValidator
    {
        public override bool IsValid(object value)
        {
            var isValid = true;
            if(value is string valueString && !string.IsNullOrWhiteSpace(valueString))
            {
                var firstCharacter = valueString.First();
                if (valueString.ToLower().StartsWith("mscrm"))
                    isValid = false;
                else if (valueString.Length < 2)
                    isValid = false;
                else if (!char.IsLetter(firstCharacter))
                    isValid = false;
                else if(valueString.Any(c => !char.IsLetterOrDigit(c)))
                    isValid = false;
            }
            return isValid;
        }

        public override string GetErrorMessage(string propertyLabel)
        {
            return "The prefix can contain only alphanumeric characters. The prefix must have at least 2 characters and start with a letter. It cannot start with \"mscrm\"";
        }
    }
}