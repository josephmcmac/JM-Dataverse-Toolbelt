#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using JosephM.Record.Application.Grid;
using JosephM.Wpf.Grid;

#endregion

namespace JosephM.Wpf.RecordEntry
{
    /// <summary>
    ///     Interaction logic for MaintainView.xaml
    /// </summary>
    public partial class GridSectionView : DynamicGridView
    {
        public GridSectionView()
        {
            InitializeComponent();
        }

        protected override DataGrid DynamicDataGrid
        {
            get { return DataGrid; }
        }

        protected IDynamicGridViewModel DynamicGridViewModel
        {
            get { return DataContext as IDynamicGridViewModel; }
        }

        private void columnHeader_Click(object sender, RoutedEventArgs e)
        {
            var columnHeader = sender as DataGridColumnHeader;
            if (columnHeader != null)
            {
                var sortMember = columnHeader.Column.SortMemberPath;
                if (sortMember != null)
                {
                    DynamicGridViewModel.DynamicGridViewModelItems.SortIt(DynamicGridViewModel, sortMember.Trim(new[] {'[', ']'}));
                }
            }
        }

    }
}