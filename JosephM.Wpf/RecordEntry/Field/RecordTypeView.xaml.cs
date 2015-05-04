#region

using System.Windows.Controls.Primitives;
using System.Windows.Data;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    public partial class RecordTypeFieldView : FieldControlBase
    {
        public RecordTypeFieldView()
        {
            InitializeComponent();
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(ComboBox, Selector.SelectedItemProperty);
        }
    }
}