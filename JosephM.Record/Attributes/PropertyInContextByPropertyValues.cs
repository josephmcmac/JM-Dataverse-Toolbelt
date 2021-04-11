using JosephM.Record.IService;
using System;
using System.Linq;

namespace JosephM.Record.Attributes
{
    /// <summary>
    ///     Defines The Property With The Attribute In Context If The Property With A Given Name Has One Of The Given Values
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class PropertyInContextByPropertyValues : PropertyInContext
    {
        public string PropertyDependency { get; set; }
        public object[] ValidValues { get; set; }

        public PropertyInContextByPropertyValues(string propertyDependency, params object[] validValues)
        {
            PropertyDependency = propertyDependency;
            ValidValues = validValues;
        }

        public override bool IsInContext(IRecordService recordService, IRecord record)
        {
            var propertyValue = record.GetField(PropertyDependency);
            return ValidValues.Any(v => PropertyInContextByPropertyValue.InContextMatch(propertyValue, v));
        }
    }
}