using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace JosephM.Wpf.RecordEntry.Field
{
    public partial class PicklistFieldView : FieldControlBase
    {
        public PicklistFieldView()
        {
            InitializeComponent();
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(ComboBox, Selector.SelectedItemProperty);
        }
    }
}