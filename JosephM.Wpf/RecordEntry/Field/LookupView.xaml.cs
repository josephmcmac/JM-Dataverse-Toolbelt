#region

using System.Windows.Data;
using System.Windows.Input;
using JosephM.Record.Application.RecordEntry.Field;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    public partial class LookupView : FieldControlBase
    {
        public LookupView()
        {
            InitializeComponent();
        }

        public IReferenceFieldViewModel ViewModel
        {
            get { return DataContext as IReferenceFieldViewModel; }
            set { DataContext = value; }
        }

        protected override Binding GetValidationBinding()
        {
            return null;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ViewModel.Search();
            if (e.Key == Key.Down)
                ViewModel.SelectLookupGrid();
            else if (e.Key == Key.K &&
                     (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                ViewModel.Search();
        }
    }
}