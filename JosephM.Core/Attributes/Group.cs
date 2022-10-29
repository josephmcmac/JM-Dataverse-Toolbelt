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
        public bool IsHiddenSection { get; private set; }



        public Group(string name, DisplayLayoutEnum displayLayout = DisplayLayoutEnum.VerticalList, int order = 0, bool selectAll = false, bool displayLabel = true, bool isHiddenSection = false)
        {
            Name = name;
            DisplayLayout = displayLayout;
            Order = order;
            SelectAll = selectAll;
            DisplayLabel = displayLabel;
            IsHiddenSection = isHiddenSection;
        }

        public enum DisplayLayoutEnum
        {
            VerticalCentered,
            VerticalList,
            HorizontalLabelAbove,
            HorizontalWrap,
            HorizontalInputOnly,
            HorizontalCenteredInputOnly,
        }
    }
}