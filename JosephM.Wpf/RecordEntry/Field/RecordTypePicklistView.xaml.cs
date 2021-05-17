using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace JosephM.Wpf.RecordEntry.Field
{
    public partial class RecordTypePicklistFieldView : FieldControlBase
    {
        public RecordTypePicklistFieldView()
        {
            InitializeComponent();
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(ComboBox, Selector.SelectedItemProperty);
        }
    }
}