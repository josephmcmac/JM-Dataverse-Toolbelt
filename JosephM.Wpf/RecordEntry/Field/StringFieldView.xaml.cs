using System.Windows.Controls;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;

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