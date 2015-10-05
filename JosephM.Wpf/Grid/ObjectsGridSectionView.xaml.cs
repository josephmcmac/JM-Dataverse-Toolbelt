#region

using System.Windows;
using System.Windows.Forms;
using JosephM.Application.ViewModel.Grid;
using UserControl = System.Windows.Controls.UserControl;

#endregion

namespace JosephM.Wpf.Grid
{
    /// <summary>
    ///     Interaction logic for SubGrid.xaml
    /// </summary>
    public partial class ObjectsGridSectionView : UserControl
    {
        public ObjectsGridSectionViewModel ViewModel
        {
            get { return (ObjectsGridSectionViewModel) DataContext; }
        }

        public ObjectsGridSectionView()
        {
            InitializeComponent();
        }

        private void CsvButtonClick(object sender, RoutedEventArgs e)
        {
            var selectFolderDialog = new FolderBrowserDialog {ShowNewFolderButton = true};
            var result = selectFolderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                ViewModel.DownloadCsv(selectFolderDialog.SelectedPath);
            }
        }
    }
}