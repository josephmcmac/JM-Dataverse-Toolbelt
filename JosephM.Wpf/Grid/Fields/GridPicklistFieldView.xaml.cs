using JosephM.Wpf.RecordEntry.Field;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace JosephM.Wpf.Grid
{
    public partial class GridPicklistFieldView : FieldControlBase
    {
        public GridPicklistFieldView()
        {
            InitializeComponent();
        }
        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(ComboBox, Selector.SelectedItemProperty);
        }
    }
}