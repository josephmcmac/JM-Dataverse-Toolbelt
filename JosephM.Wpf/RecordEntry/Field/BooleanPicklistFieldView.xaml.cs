using System.Windows.Controls;
using System.Windows.Data;

namespace JosephM.Wpf.RecordEntry.Field
{
    /// <summary>
    ///     Interaction logic for XrmIntegerTextBox.xaml
    /// </summary>
    public partial class BooleanPicklistFieldView : FieldControlBase
    {
        public BooleanPicklistFieldView()
        {
            InitializeComponent();
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(ComboBox, TextBox.TextProperty);
        }
    }
}