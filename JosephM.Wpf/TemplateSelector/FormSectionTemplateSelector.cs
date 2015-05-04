#region

using System;
using System.Windows;
using System.Windows.Controls;
using JosephM.Record.Application.RecordEntry.Section;

#endregion

namespace JosephM.Wpf.TemplateSelector
{
    public class FormSectionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate FieldSectionTemplate { get; set; }
        public DataTemplate GridSectionTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is FieldSectionViewModel)
                return FieldSectionTemplate;
            if (item is GridSectionViewModel)
                return GridSectionTemplate;
            throw new ArgumentOutOfRangeException(string.Concat("No template defined for the type",
                item == null ? "null" : item.GetType().FullName));
        }
    }
}