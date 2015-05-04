#region

using System.Windows.Controls;
using JosephM.Wpf.Grid;

#endregion

namespace JosephM.Wpf.RecordEntry
{
    /// <summary>
    ///     Interaction logic for MaintainView.xaml
    /// </summary>
    public partial class SearchResultView : DynamicGridView
    {
        public SearchResultView()
        {
            InitializeComponent();
        }

        protected override DataGrid DynamicDataGrid
        {
            get { return DataGrid; }
        }
    }
}