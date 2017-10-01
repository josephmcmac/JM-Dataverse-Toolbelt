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
        public DisplayLayoutEnum DisplayLayout { get; private set; }
        public int Order { get; private set; }

        public Group(string name, DisplayLayoutEnum displayLayout = DisplayLayoutEnum.VerticalList, int order = 0)
        {
            Name = name;
            DisplayLayout = displayLayout;
            Order = order;
        }

        public Group(string name, bool wrapHorizontal, int order = 10000)
            : this(name, displayLayout: wrapHorizontal ? DisplayLayoutEnum.HorizontalWrap : DisplayLayoutEnum.VerticalList, order: order)
        {
        }

        public enum DisplayLayoutEnum
        {
            VerticalList,
            HorizontalWrap,
            HorizontalInputOnly
        }
    }
}