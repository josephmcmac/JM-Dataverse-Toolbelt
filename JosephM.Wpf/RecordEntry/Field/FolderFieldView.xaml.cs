#region

using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using JosephM.Core.Constants;
using Binding = System.Windows.Data.Binding;
using TextBox = System.Windows.Controls.TextBox;

#endregion

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

        public string FileMask
        {
            get { return FileMasks.ExcelFile; }
        }

        private void selectButtonClick(object sender, RoutedEventArgs e)
        {
            var selectFolderDialog = new FolderBrowserDialog {ShowNewFolderButton = true};
            var result = selectFolderDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                FileNameTextBox.Text = selectFolderDialog.SelectedPath;
            }
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(FileNameTextBox, TextBox.TextProperty);
        }
    }
}