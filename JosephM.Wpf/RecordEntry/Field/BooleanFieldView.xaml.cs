using System.Windows.Controls.Primitives;
using System.Windows.Data;
using JosephM.Application.ViewModel.RecordEntry.Field;

namespace JosephM.Wpf.RecordEntry.Field
{
    /// <summary>
    ///     Interaction logic for XrmIntegerTextBox.xaml
    /// </summary>
    public partial class BooleanFieldView : FieldControlBase
    {
        public BooleanFieldView()
        {
            InitializeComponent();
        }

        public IntegerFieldViewModel ViewModel
        {
            get { return DataContext as IntegerFieldViewModel; }
            set { DataContext = value; }
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(CheckBox, ToggleButton.IsCheckedProperty);
        }
    }
}