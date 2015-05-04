#region

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using JosephM.Core.Constants;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    /// <summary>
    ///     Interaction logic for FileSelector.xaml
    /// </summary>
    public partial class ExcelFileFieldView : FieldControlBase
    {
        public ExcelFileFieldView()
        {
            InitializeComponent();
        }

        public string FileMask
        {
            get { return FileMasks.ExcelFile; }
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