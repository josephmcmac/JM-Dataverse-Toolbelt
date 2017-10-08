#region

using System;
using System.Windows;
using System.Windows.Controls;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.Shared;
using JosephM.Application.ViewModel.Query;

#endregion

namespace JosephM.Wpf.TemplateSelector
{
    public class ConditionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ConditionTemplate { get; set; }
        public DataTemplate FilterConditionsTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is ConditionViewModel)
                return ConditionTemplate;
            if (item is FilterConditionsViewModel)
                return FilterConditionsTemplate;
            throw new ArgumentOutOfRangeException(string.Concat("No template defined for the type",
                item == null ? "null" : item.GetType().FullName));
        }
    }
}