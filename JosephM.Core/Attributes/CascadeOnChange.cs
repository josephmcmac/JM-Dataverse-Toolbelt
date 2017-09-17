using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Core.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class CascadeOnChange : Attribute
    {
        public string TargetProperty { get; set; }

        public CascadeOnChange(string targetProperty)
        {
            TargetProperty = targetProperty;
        }

    }
}