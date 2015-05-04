#region

using System;
using System.Windows;
using System.Windows.Controls;
using JosephM.Record.Application.Grid;
using JosephM.Record.Application.Shared;

#endregion

namespace JosephM.Wpf.TemplateSelector
{
    public class CompletionItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ObjectsGridSectionTemplate { get; set; }
        public DataTemplate HeadingTemplate { get; set; }
        public DataTemplate XrmButtonTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is ObjectsGridSectionViewModel)
                return ObjectsGridSectionTemplate;
            if (item is HeadingViewModel)
                return HeadingTemplate;
            if (item is XrmButtonViewModel)
                return XrmButtonTemplate;
            throw new ArgumentOutOfRangeException(string.Concat("No template defined for the type",
                item == null ? "null" : item.GetType().FullName));
        }
    }
}