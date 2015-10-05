using System;
using JosephM.Core.Attributes;
using JosephM.Record.Extentions;

namespace JosephM.Record.Attributes
{
    /// <summary>
    ///     Attribute To Define A Property As Cascading The Record Type To Another Property
    ///     Initally Used For Cacading A selected Record Type To A Record Field Or Lookup Property
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class LookupConnectionFor : ConnectionFor, IStoredObjectFields
    {
        private IStoredObjectFields FieldConfig { get; set; }
        public string AssemblyField { get { return FieldConfig.AssemblyField; }}
        public string TypeQualfiedNameField { get { return FieldConfig.TypeQualfiedNameField; } }
        public string TypeField { get { return FieldConfig.TypeField; } }
        public string RecordType { get { return FieldConfig.RecordType; } }
        public string ValueField { get { return FieldConfig.ValueField; } }

        public LookupConnectionFor(string property, IStoredObjectFields fieldConfig)
            : base(property)
        {
            FieldConfig = fieldConfig;
        }
    }
}