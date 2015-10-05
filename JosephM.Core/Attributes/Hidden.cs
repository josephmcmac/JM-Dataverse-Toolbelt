using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define An Alternative Display Name For A Class Type Through The TypeEntentions.GetDisplayName Method
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class | AttributeTargets.Property)]
    public class HiddenAttribute : PropertyInContext
    {
        public override bool IsInContext(object instance)
        {
            return false;
        }
    }
}