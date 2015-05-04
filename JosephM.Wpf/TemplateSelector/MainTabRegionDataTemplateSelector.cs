#region

using System;
using System.Windows;
using System.Windows.Controls;
using JosephM.Record.Application.Dialog;
using JosephM.Record.Application.HTML;
using JosephM.Record.Application.Navigation;
using JosephM.Record.Application.RecordEntry.Form;

#endregion

namespace JosephM.Wpf.TemplateSelector
{
    public class MainTabRegionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RecordEntryViewTemplate { get; set; }
        public DataTemplate DialogTemplate { get; set; }
        public DataTemplate NavigationErrorTemplate { get; set; }
        public DataTemplate HtmlFileTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is RecordEntryFormViewModel)
                return RecordEntryViewTemplate;
            if (item is NavigationErrorViewModel)
                return NavigationErrorTemplate;
            if (item is DialogViewModel)
                return DialogTemplate;
            if (item is HtmlFileModel)
                return HtmlFileTemplate;
            throw new ArgumentOutOfRangeException(
                string.Format(
                    "No Template Defined For The Type :{0} - If Navigation Check RegisterTypeForNavigation Has Been Called On The Container For The Type",
                    item == null ? "null" : item.GetType().FullName));
        }
    }
}