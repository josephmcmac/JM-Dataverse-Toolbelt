using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace JosephM.Wpf.RecordEntry.Field
{
    /// <summary>
    ///     Interaction logic for FileSelector.xaml
    /// </summary>
    public partial class FolderFieldView : FieldControlBase
    {
        public FolderFieldView()
        {
            InitializeComponent();
        }

        private void selectButtonClick(object sender, RoutedEventArgs e)
        {
            var selectFolderDialog = new System.Windows.Forms.FolderBrowserDialog { ShowNewFolderButton = true};
            var result = selectFolderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                FileNameTextBox.Text = selectFolderDialog.SelectedPath;
            }
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(FileNameTextBox, TextBox.TextProperty);
        }

        private void DropFile(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var data = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        FileNameTextBox.Text = item;
                    }
                }
            }
        }

        private void TextBoxDragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }
    }
}