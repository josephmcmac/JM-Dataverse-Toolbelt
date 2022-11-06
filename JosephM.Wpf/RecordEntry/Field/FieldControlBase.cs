using System.Windows.Controls;
using System.Windows.Data;

namespace JosephM.Wpf.RecordEntry.Field
{
    public abstract class FieldControlBase : UserControl
    {
        protected abstract Binding GetValidationBinding();
    }
}