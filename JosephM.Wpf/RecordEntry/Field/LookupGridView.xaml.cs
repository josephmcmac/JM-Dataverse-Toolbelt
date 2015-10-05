#region

using System.Windows.Controls;
using System.Windows.Input;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Wpf.ControlExtentions;
using JosephM.Wpf.Grid;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    /// <summary>
    ///     Interaction logic for MaintainView.xaml
    /// </summary>
    public partial class LookupGridView : UserControl
    {
        public LookupGridView()
        {
            InitializeComponent();
            DataGrid.PreviewKeyDown += OnPreviewKeyDown;
            DataGrid.SelectionMode = DataGridSelectionMode.Single;
        }

        protected LookupGridViewModel ViewModel
        {
            get { return (LookupGridViewModel) DataContext; }
        }

        protected DataGrid DataGrid
        {
            get { return this.MyGrid.DataGrid; }
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