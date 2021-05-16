using JosephM.Application.ViewModel.RecordEntry.Field;
using System.Windows;
using System.Windows.Controls;

namespace JosephM.Wpf.TemplateSelector
{
    public class LabelledFieldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LabelledFieldTemplate { get; set; }
        public DataTemplate LabelledAboveFieldTemplate { get; set; }
        public DataTemplate UnlabelledFieldTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            if (item is FieldViewModelBase fm && !fm.DisplayLabel)
            {
                return UnlabelledFieldTemplate;
            }
            if (item is EnumerableFieldViewModel)
            {
                return LabelledAboveFieldTemplate;
            }
            return LabelledFieldTemplate;
        }
    }
}