#region

using System;
using System.Windows;
using System.Windows.Controls;
using JosephM.Application.ViewModel.RecordEntry.Section;

#endregion

namespace JosephM.Wpf.TemplateSelector
{
    public class FormSectionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FieldSectionTemplate { get; set; }
        public DataTemplate FieldSectionHorozontalLabelAboveTemplate { get; set; }
        public DataTemplate FieldSectionCenteredTemplate { get; set; }
        public DataTemplate FieldSectionCompactTemplate { get; set; }
        public DataTemplate FieldSectionInputOnlyTemplate { get; set; }
        public DataTemplate FieldSectionHorizontalCenteredInputOnlyTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is FieldSectionViewModel)
            {
                var vm = (FieldSectionViewModel)item;
                switch(vm.DisplayLayout)
                {
                    case Core.Attributes.Group.DisplayLayoutEnum.HorizontalInputOnly:
                        {
                            return FieldSectionInputOnlyTemplate;
                        }
                    case Core.Attributes.Group.DisplayLayoutEnum.HorizontalWrap:
                        {
                            return FieldSectionCompactTemplate;
                        }
                    case Core.Attributes.Group.DisplayLayoutEnum.VerticalCentered:
                        {
                            return FieldSectionCenteredTemplate;
                        }
                    case Core.Attributes.Group.DisplayLayoutEnum.HorizontalLabelAbove:
                        {
                            return FieldSectionHorozontalLabelAboveTemplate;
                        }
                    case Core.Attributes.Group.DisplayLayoutEnum.HorizontalCenteredInputOnly:
                        {
                            return FieldSectionHorizontalCenteredInputOnlyTemplate;
                        }
                    case Core.Attributes.Group.DisplayLayoutEnum.VerticalList:
                        {
                            return FieldSectionTemplate;
                        }
                }
                return FieldSectionCenteredTemplate;
            }
            throw new ArgumentOutOfRangeException(string.Concat("No template defined for the type",
                item == null ? "null" : item.GetType().FullName));
        }
    }
}