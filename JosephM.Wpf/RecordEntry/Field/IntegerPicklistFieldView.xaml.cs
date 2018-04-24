#region

using System.Windows.Controls;
using System.Windows.Data;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    /// <summary>
    ///     Interaction logic for XrmIntegerTextBox.xaml
    /// </summary>
    public partial class IntegerPicklistFieldView : FieldControlBase
    {
        public IntegerPicklistFieldView()
        {
            InitializeComponent();
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(ComboBox, TextBox.TextProperty);
        }
    }
}