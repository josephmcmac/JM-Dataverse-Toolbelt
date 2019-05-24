using JosephM.Core.Attributes;
using System;
using System.Linq;

namespace JosephM.XrmModule.Crud.Validations
{
    /// <summary>
    ///     Attribute To Define A Property As Required To Be Non-Empty To Be Valid
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class SolutionOrPublisherNameValidation : PropertyValidator
    {
        public override bool IsValid(object value)
        {
            var isValid = true;
            if(value is string valueString && !string.IsNullOrWhiteSpace(valueString))
            {
                var firstCharacter = valueString.First();
                if (!char.IsLetter(firstCharacter) && !(firstCharacter == '_'))
                    isValid = false;
                else if(valueString.Any(c => !char.IsLetterOrDigit(c) && !(c == '_')))
                    isValid = false;
            }
            return isValid;
        }

        public override string GetErrorMessage(string propertyLabel)
        {
            return "Invalid character specified. Only characters within the ranges [A-Z], [a-z], [0-9] or _ are allowed. The first character may only be in the ranges [A-Z], [a-z] or _";
        }
    }
}