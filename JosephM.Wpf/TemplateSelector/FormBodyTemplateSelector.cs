using System;
using System.Windows;
using System.Windows.Controls;
using JosephM.Application.ViewModel.RecordEntry.Form;

namespace JosephM.Wpf.TemplateSelector
{
    public class FormBodyTemplateSelector : DataTemplateSelector
    {
        public DataTemplate GridOnlyBodyTemplate { get; set; }
        public DataTemplate SectionsBodyTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item == null)
                return base.SelectTemplate(item, container);
            if (item is RecordEntryFormViewModel revm)
            {
                if (revm.GridOnlyField != null)
                    return GridOnlyBodyTemplate;
                else
                    return SectionsBodyTemplate;
            }
            throw new ArgumentOutOfRangeException(string.Concat("No template defined for the type",
                item == null ? "null" : item.GetType().FullName));
        }
    }
}