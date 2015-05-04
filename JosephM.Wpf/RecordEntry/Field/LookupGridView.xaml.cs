#region

using System.Windows.Controls;
using System.Windows.Input;
using JosephM.Record.Application.RecordEntry.Field;
using JosephM.Wpf.Grid;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    /// <summary>
    ///     Interaction logic for MaintainView.xaml
    /// </summary>
    public partial class LookupGridView : DynamicGridView
    {
        public LookupGridView()
        {
            InitializeComponent();
        }

        protected override DataGrid DynamicDataGrid
        {
            get { return DataGrid; }
        }

        protected LookupGridViewModel ViewModel
        {
            get { return (LookupGridViewModel) DataContext; }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                ViewModel.MoveDown();
                if (DataGrid.SelectedItem != null)
                    DataGrid.ScrollIntoView(DataGrid.SelectedItem);
            }
            else if (e.Key == Key.Up)
            {
                ViewModel.MoveUp();
                if (DataGrid.SelectedItem != null)
                    DataGrid.ScrollIntoView(DataGrid.SelectedItem);
            }

            else if (e.Key == Key.Enter)
                ViewModel.SetLookupToSelectedRow();
        }
    }
}