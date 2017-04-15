using System;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Attribute To Define An Alternative Display Name For A Class Type Through The TypeEntentions.GetDisplayName Method
    /// </summary>
    [AttributeUsage(AttributeTargets.All,
        AllowMultiple = true)]
    public class Group : Attribute
    {
        public string Name { get; private set; }
        public bool WrapHorizontal { get; private set; }
        public int Order { get; private set; }

        public Group(string name)
        {
            Name = name;
        }

        public Group(string name, bool wrapHorizontal, int order = 10000)
        {
            Name = name;
            WrapHorizontal = wrapHorizontal;
            Order = order;
        }
    }
}