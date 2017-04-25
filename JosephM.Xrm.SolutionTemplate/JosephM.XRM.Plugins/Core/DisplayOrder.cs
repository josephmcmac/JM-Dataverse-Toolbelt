using System;

namespace $safeprojectname$.Core
{
    /// <summary>
    ///     Attribute To Define An Alternative Display Name For A Class Type Through The TypeEntentions.GetDisplayName Method
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class DisplayOrder : Attribute
    {
        public int Order { get; private set; }

        public DisplayOrder(int order)
        {
            Order = order;
        }
    }
}