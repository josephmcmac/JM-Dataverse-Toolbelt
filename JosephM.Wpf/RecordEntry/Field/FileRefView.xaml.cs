#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using JosephM.Record.Application.RecordEntry.Field;
using Microsoft.Win32;
using JosephM.Core.Constants;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    /// <summary>
    ///     Interaction logic for FileSelector.xaml
    /// </summary>
    public partial class FileRefFieldView : FieldControlBase
    {
        public FileRefFieldView()
        {
            InitializeComponent();
        }

        public FileRefFieldViewModel ViewModel
        {
            get { return DataContext as FileRefFieldViewModel; }
            set { DataContext = value; }
        }

        public string FileMask
        {
            get { return ViewModel == null ? null : ViewModel.FileMask; }
        }

        private void selectButtonClick(object sender, RoutedEventArgs e)
        {
            var selectFileDialog = new OpenFileDialog {Filter = FileMask};
            var selected = selectFileDialog.ShowDialog();
            if (selected ?? false)
            {
                FileNameTextBox.Text = selectFileDialog.FileName;
            }
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(FileNameTextBox, TextBox.TextProperty);
        }
    }
}