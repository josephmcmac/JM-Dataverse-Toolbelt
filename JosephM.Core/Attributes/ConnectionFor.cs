using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define A Property As Cascading The Record Type To Another Property
    ///     Initally Used For Cacading A selected Record Type To A Record Field Or Lookup Property
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class ConnectionFor : Attribute
    {
        public Type ConnectionType { get; private set; }
        public string Property { get; private set; }

        public ConnectionFor(string property)
        {
            Property = property;
        }

        public ConnectionFor(string property, Type connectionType)
        {
            ConnectionType = connectionType;
            Property = property;
        }
    }
}