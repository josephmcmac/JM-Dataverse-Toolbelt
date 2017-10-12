using System;
using JosephM.Record.Query;

namespace JosephM.Record.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class LookupFieldCascade : Attribute
    {
        public string TargetProperty { get; set; }
        public string SourceRecordField { get; set; }

        public LookupFieldCascade(string targetProperty, string sourceRecordField)
        {
            TargetProperty = targetProperty;
            SourceRecordField = sourceRecordField;
        }
    }
}
