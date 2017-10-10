using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Base Class For Logic To Define A Property As Having A Valid State
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = true)]
    public abstract class PropertyValidator : Attribute
    {
        public abstract bool IsValid(object value);

        public abstract string GetErrorMessage(string propertyLabel);
    }
}