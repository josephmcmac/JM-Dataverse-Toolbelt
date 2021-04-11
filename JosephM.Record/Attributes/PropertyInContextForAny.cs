using System;
using System.Linq;
using JosephM.Core.Extentions;
using JosephM.Record.IService;

namespace JosephM.Record.Attributes
{
    /// <summary>
    ///     Defines The Property With The Attribute In Context If Any Of The Boolean Property Values Is True
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class PropertyInContextForAny : PropertyInContext
    {
        public string[] BooleanProperties { get; set; }

        public PropertyInContextForAny(params string[] booleanProperties)
        {
            BooleanProperties = booleanProperties;
        }

        public override bool IsInContext(IRecordService recordService, IRecord record)
        {
            return BooleanProperties.Any(p => (bool)record.GetField(p));
        }
    }
}