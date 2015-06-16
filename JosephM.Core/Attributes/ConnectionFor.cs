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
        public string LookupProperty { get; private set; }

        public ConnectionFor(string lookupProperty)
        {
            LookupProperty = lookupProperty;
        }

        public IEnumerable<string> PropertyPaths
        {
            get
            {
                return LookupProperty.Split('.');
            }
        }

        public string PropertyPath1
        {
            get
            {
                return PropertyPaths.ElementAt(0);
            }
        }

        public string PropertyPath2
        {
            get
            {
                return PropertyPaths.ElementAt(1);
            }
        }
    }
}