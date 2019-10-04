using System;

namespace JosephM.Record.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public class ConnectionConstructor : Attribute
    {
    }
}
