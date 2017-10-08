#region

using System.Windows.Controls;
using System.Windows.Data;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    /// <summary>
    ///     Interaction logic for DecimalFieldView.xaml
    /// </summary>
    public partial class DecimalFieldView : FieldControlBase
    {
        public DecimalFieldView()
        {
            InitializeComponent();
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(DecimalTextBox, TextBox.TextProperty);
        }
    }
}