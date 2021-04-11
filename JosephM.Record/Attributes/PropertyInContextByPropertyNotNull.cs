using System;
using JosephM.Core.Extentions;
using JosephM.Record.IService;

namespace JosephM.Record.Attributes
{
    /// <summary>
    ///     Defines The Property With The Attribute In Context If The Property With A Given Name Is Not Empty
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class PropertyInContextByPropertyNotNull : PropertyInContext
    {
        public string PropertyDependency { get; set; }

        public PropertyInContextByPropertyNotNull(string prepertyDependency)
        {
            PropertyDependency = prepertyDependency;
        }

        public override bool IsInContext(IRecordService recordService, IRecord record)
        {
            var propertyValue = record.GetField(PropertyDependency);
            return (propertyValue != null);
        }
    }
}