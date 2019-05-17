using JosephM.Application.ViewModel.RecordEntry.Field;
using System.Windows;
using System.Windows.Controls;

namespace JosephM.Wpf.TemplateSelector
{
    public class LabelledFieldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LabelledFieldTemplate { get; set; }
        public DataTemplate UnlabelledFieldTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is FieldViewModelBase && !((FieldViewModelBase)item).DisplayLabel)
                return UnlabelledFieldTemplate;
            return LabelledFieldTemplate;
        }
    }
}