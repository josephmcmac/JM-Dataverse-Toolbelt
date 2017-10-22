#region

using System;
using System.Windows;
using System.Windows.Controls;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Shared;

#endregion

namespace JosephM.Wpf.TemplateSelector
{
    public class DialogTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RecordEntryTemplate { get; set; }
        public DataTemplate RecordEntryTemplateWindowMinimum { get; set; }
        public DataTemplate ProgressTemplate { get; set; }
        public DataTemplate CompletionTemplate { get; set; }
        public DataTemplate LoadingTemplate { get; set; }
        public DataTemplate ListViewTemplate { get; set; }
        public DataTemplate QueryViewTemplate { get; set; }
        public DataTemplate QueryViewTemplateWindowMinimum { get; set; }
        public DataTemplate DialogTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is ProgressControlViewModel)
                return ProgressTemplate;
            if (item is RecordEntryFormViewModel)
            {
                var re = (RecordEntryFormViewModel)item;
                if (re.ApplicationController?.ForceElementWindowHeight ?? false)
                    return RecordEntryTemplateWindowMinimum;
                else
                    return RecordEntryTemplate;
            }
            if (item is CompletionScreenViewModel)
                return CompletionTemplate;
            if (item is ListViewModel)
                return ListViewTemplate;
            if (item is LoadingViewModel)
                return LoadingTemplate;
            if (item is QueryViewModel)
            {
                var qv = (QueryViewModel)item;
                if (qv.ApplicationController?.ForceElementWindowHeight ?? false)
                    return QueryViewTemplateWindowMinimum;
                else
                    return QueryViewTemplate;
            }
            if (item is DialogViewModel)
                return DialogTemplate;
            throw new ArgumentOutOfRangeException(string.Concat("No template defined for the type",
                item == null ? "null" : item.GetType().FullName));
        }
    }
}