#region

using JosephM.Application.ViewModel.Grid;
using System.Windows.Controls;
using System.Windows.Input;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

#endregion

namespace JosephM.Wpf.Grid
{
    /// <summary>
    ///     Interaction logic for MaintainView.xaml
    /// </summary>
    public partial class GridQueryView : UserControl
    {
        public QueryViewModel ViewModel
        {
            get { return DataContext as QueryViewModel; }
            set { DataContext = value; }
        }

        public GridQueryView()
        {
            InitializeComponent();
        }

        private void OnQuickFindKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ViewModel.QuickFind();
            else if (e.Key == Key.K &&
                     (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                ViewModel.QuickFind();
        }
    }
}