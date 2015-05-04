#region

using System.Windows;
using System.Windows.Controls;

#endregion

namespace JosephM.Wpf.TemplateSelector
{
    public class LabelledFieldTemplateSelector : DataTemplateSelector
    {
        public DataTemplate LabelledFieldTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item,
            DependencyObject container)
        {
            return LabelledFieldTemplate;
        }
    }
}