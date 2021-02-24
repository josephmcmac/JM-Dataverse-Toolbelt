using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define A Property As Cascading The Record Type To Another Property
    ///     Initally Used For Cacading A selected Record Type To A Record Field Or Lookup Property
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public class TargetTypesFor : Attribute
    {
        public string Property { get; private set; }

        public TargetTypesFor(string property)
        {
            Property = property;
        }
    }
}