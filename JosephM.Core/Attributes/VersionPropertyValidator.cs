using System;
using System.Linq;

namespace JosephM.Core.Attributes
{
    /// <summary>
    /// Validates version field format
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class VersionPropertyValidator : PropertyValidator
    {
        public override bool IsValid(object value)
        {
            var isValid = true;
            if(value is string valueString && !string.IsNullOrWhiteSpace(valueString))
            {
                var splitValues = valueString.Split('.');
                return splitValues.Count() == 4
                    && splitValues.All(s => int.TryParse(s, out int z));
            }
            return isValid;
        }

        public override string GetErrorMessage(string propertyLabel)
        {
            return $"{propertyLabel} must be a version with the following format: major.minor.build.revision, for example, 1.0.0.0.";
        }

        public override bool PreventSettingProperty => true;
    }
}