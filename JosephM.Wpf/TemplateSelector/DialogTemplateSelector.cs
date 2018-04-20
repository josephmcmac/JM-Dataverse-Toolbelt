#region

using System;
using System.Windows;
using System.Windows.Controls;
using JosephM.Application.ViewModel;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.Query;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Shared;

#endregion

namespace JosephM.Wpf.TemplateSelector
{
    public class DialogTemplateSelector : DataTemplateSelector
    {
        public DataTemplate RecordEntryTemplateTabSize { get; set; }
        public DataTemplate RecordEntryTemplateWindowSize { get; set; }
        public DataTemplate ProgressTemplate { get; set; }
        public DataTemplate CompletionTemplateTabSize { get; set; }
        public DataTemplate CompletionTemplateWindowSize { get; set; }
        public DataTemplate LoadingTemplate { get; set; }
        public DataTemplate QueryViewTemplateTabSize { get; set; }
        public DataTemplate QueryViewTemplateWindowSize { get; set; }
        public DataTemplate DialogTemplate { get; set; }
        public DataTemplate MultiSelectDialogTemplateTabSize { get; set; }
        public DataTemplate MultiSelectDialogTemplateWindowSize { get; set; }
        public DataTemplate ColumnEditDialogTemplateTabSize { get; set; }
        public DataTemplate ColumnEditDialogTemplateWindowSize { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if(item is ViewModelBase)
            {
                var viewModel = (ViewModelBase)item;
                if (viewModel.ApplicationController?.ForceElementWindowHeight ?? false)
                {
                    if (item is RecordEntryFormViewModel)
                        return RecordEntryTemplateWindowSize;
                    if (item is CompletionScreenViewModel)
                        return CompletionTemplateWindowSize;
                    if (item is QueryViewModel)
                        return QueryViewTemplateWindowSize;
                    if (item is IMultiSelectDialog)
                        return MultiSelectDialogTemplateWindowSize;
                    if (item is ColumnEditDialogViewModel)
                        return ColumnEditDialogTemplateWindowSize;
                }
                else
                {
                    if (item is RecordEntryFormViewModel)
                        return RecordEntryTemplateTabSize;
                    if (item is CompletionScreenViewModel)
                        return CompletionTemplateTabSize;
                    if (item is QueryViewModel)
                        return QueryViewTemplateTabSize;
                    if (item is IMultiSelectDialog)
                        return MultiSelectDialogTemplateTabSize;
                    if (item is ColumnEditDialogViewModel)
                        return ColumnEditDialogTemplateTabSize;
                }
            }
            if (item is ProgressControlViewModel)
                return ProgressTemplate;
            if (item is LoadingViewModel)
                return LoadingTemplate;
            if (item is DialogViewModel)
                return DialogTemplate;

            throw new ArgumentOutOfRangeException(string.Concat("No template defined for the type",
                item == null ? "null" : item.GetType().FullName));
        }
    }
}