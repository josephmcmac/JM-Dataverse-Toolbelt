using JosephM.Application.ViewModel.RecordEntry.Field;
using System.Windows;
using System.Windows.Controls;

namespace JosephM.Wpf.TemplateSelector
{
    public class LabelledAboveFieldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LabelledAboveFieldTemplate { get; set; }
        public DataTemplate UnlabelledFieldTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is FieldViewModelBase fm && !fm.DisplayLabel)
            {
                return UnlabelledFieldTemplate;
            }
            return LabelledAboveFieldTemplate;
        }
    }
}