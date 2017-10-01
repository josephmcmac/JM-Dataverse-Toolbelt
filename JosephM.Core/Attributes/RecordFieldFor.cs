using System;
using System.Collections.Generic;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define A Property As Cascading The Field To Another Property
    ///     Initally Used For Query Condition Values Being For The Field Type
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class RecordFieldFor : Attribute
    {
        public string ObjectProperty { get; private set; }

        public RecordFieldFor(string objectProperty)
        {
            ObjectProperty = objectProperty;
        }

        public IEnumerable<string> PropertyPaths
        {
            get
            {
                return ObjectProperty.Split('.');
            }
        }
    }
}