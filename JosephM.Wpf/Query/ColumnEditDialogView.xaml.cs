#region

using JosephM.Application.ViewModel.Query;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

#endregion

namespace JosephM.Wpf.Query
{
    /// <summary>
    ///     Interaction logic for FormSectionView.xaml
    /// </summary>
    public partial class ColumnEditDialogView : UserControl
    {
        public ColumnEditDialogView()
        {
            InitializeComponent();
        }

        private ColumnEditDialogViewModel ColumnEditDialogViewModel
        {
            get
            {
                return DataContext as ColumnEditDialogViewModel;
            }
        }

        private void SelectMouseMoveEvent(object sender, MouseEventArgs e)
        {
            var whatIsIt = sender as FrameworkElement;
            if (whatIsIt != null && e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(whatIsIt, whatIsIt.DataContext, DragDropEffects.Move);
            }
        }

        private void SelectDropBefore(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(typeof(ColumnEditDialogViewModel.SelectableColumn)))
            {
                var draggedItem = (ColumnEditDialogViewModel.SelectableColumn)e.Data.GetData(typeof(ColumnEditDialogViewModel.SelectableColumn));
                var thisItem = sender as FrameworkElement;
                var target = thisItem.DataContext as ColumnEditDialogViewModel.SelectableColumn;
                ColumnEditDialogViewModel.AddCurrentItem(draggedItem, target: target, isAfter: false);
            }
        }

        private void SelectDropAfter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ColumnEditDialogViewModel.SelectableColumn)))
            {
                var draggedItem = (ColumnEditDialogViewModel.SelectableColumn)e.Data.GetData(typeof(ColumnEditDialogViewModel.SelectableColumn));
                var thisItem = sender as FrameworkElement;
                var target = thisItem.DataContext as ColumnEditDialogViewModel.SelectableColumn;
                ColumnEditDialogViewModel.AddCurrentItem(draggedItem, target: target, isAfter: true);
            }
        }
    }
}