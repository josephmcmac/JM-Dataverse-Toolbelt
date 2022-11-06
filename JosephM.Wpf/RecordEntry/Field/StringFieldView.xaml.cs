using JosephM.Application.ViewModel.RecordEntry.Field;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Binding = System.Windows.Data.Binding;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace JosephM.Wpf.RecordEntry.Field
{
    public partial class StringFieldView : FieldControlBase
    {
        public StringFieldView()
        {
            InitializeComponent();
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(TextBox, TextBox.TextProperty);
        }
    }
}