#region

using System.Windows.Controls;
using System.Windows.Data;
using JosephM.Record.Application.RecordEntry.Field;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    /// <summary>
    ///     Interaction logic for XrmIntegerTextBox.xaml
    /// </summary>
    public partial class DateFieldView : FieldControlBase
    {
        public DateFieldView()
        {
            InitializeComponent();
        }

        public DateFieldViewModel XrmFieldDateViewModel
        {
            get { return DataContext as DateFieldViewModel; }
            set { DataContext = value; }
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(DatePicker, DatePicker.SelectedDateProperty);
        }
    }
}