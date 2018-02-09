#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.Navigation;
using JosephM.Application.ViewModel.Query;
using JosephM.Application.ViewModel.RecordEntry.Form;
using System;
using System.Windows;
using System.Windows.Controls;

#endregion

namespace JosephM.Wpf.TemplateSelector
{
    public class MainTabRegionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate QueryViewTemplate { get; set; }
        public DataTemplate RecordEntryViewTemplate { get; set; }
        public DataTemplate DialogTemplate { get; set; }
        public DataTemplate NavigationErrorTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is QueryViewModel)
                return QueryViewTemplate;
            if (item is RecordEntryFormViewModel)
                return RecordEntryViewTemplate;
            if (item is NavigationErrorViewModel)
                return NavigationErrorTemplate;
            if (item is DialogViewModel)
                return DialogTemplate;
            throw new ArgumentOutOfRangeException(
                string.Format(
                    "No Template Defined For The Type :{0} - If Navigation Check RegisterTypeForNavigation Has Been Called On The Container For The Type",
                    item == null ? "null" : item.GetType().FullName));
        }
    }
}