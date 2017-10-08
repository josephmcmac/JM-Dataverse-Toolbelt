using System;

namespace JosephM.Record.Attributes
{
    [AttributeUsage(
    AttributeTargets.Property,
    AllowMultiple = false)]
    public class NotSearchable : Attribute
    {
    }
}
