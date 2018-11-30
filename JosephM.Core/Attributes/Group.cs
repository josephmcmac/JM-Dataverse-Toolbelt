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
        public bool SelectAll { get; private set; }
        public bool DisplayLabel { get; private set; }

        public Group(string name, DisplayLayoutEnum displayLayout = DisplayLayoutEnum.VerticalList, int order = 0, bool selectAll = false, bool displayLabel = true)
        {
            Name = name;
            DisplayLayout = displayLayout;
            Order = order;
            SelectAll = selectAll;
            DisplayLabel = displayLabel;
        }

        public Group(string name, bool wrapHorizontal, int order = 10000, bool selectAll = false, bool displayLabel = true)
            : this(name, displayLayout: wrapHorizontal ? DisplayLayoutEnum.HorizontalWrap : DisplayLayoutEnum.VerticalList, order: order, selectAll: selectAll, displayLabel: displayLabel)
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