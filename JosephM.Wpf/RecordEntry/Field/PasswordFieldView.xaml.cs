#region

using System.Windows.Data;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    /// <summary>
    ///     Interaction logic for XrmIntegerTextBox.xaml
    /// </summary>
    public partial class PasswordFieldView : FieldControlBase
    {
        public PasswordFieldView()
        {
            InitializeComponent();
        }

        protected override Binding GetValidationBinding()
        {
            return null;
        }
    }
}