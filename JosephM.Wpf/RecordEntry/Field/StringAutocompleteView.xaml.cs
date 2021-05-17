using System.Windows.Controls;
using System.Windows.Input;
using JosephM.Application.ViewModel.RecordEntry.Field;

namespace JosephM.Wpf.RecordEntry.Field
{
    /// <summary>
    ///     Interaction logic for MaintainView.xaml
    /// </summary>
    public partial class StringAutocompleteView : UserControl
    {
        public StringAutocompleteView()
        {
            InitializeComponent();
            DataGrid.PreviewKeyDown += OnPreviewKeyDown;
            DataGrid.SelectionMode = DataGridSelectionMode.Single;
        }

        protected AutocompleteViewModel ViewModel
        {
            get { return (AutocompleteViewModel) DataContext; }
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
                ViewModel.SetToSelectedRow();
        }
    }
}