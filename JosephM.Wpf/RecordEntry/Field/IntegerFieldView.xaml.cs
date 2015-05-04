#region

using System.Windows.Controls;
using System.Windows.Data;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    /// <summary>
    ///     Interaction logic for XrmIntegerTextBox.xaml
    /// </summary>
    public partial class IntegerFieldView : FieldControlBase
    {
        public IntegerFieldView()
        {
            InitializeComponent();
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(IntegerTextBox, TextBox.TextProperty);
        }
    }
}