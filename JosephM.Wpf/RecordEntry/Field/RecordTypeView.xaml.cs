using JosephM.Application.ViewModel.RecordEntry.Field;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace JosephM.Wpf.RecordEntry.Field
{
    public partial class RecordTypeFieldView : FieldControlBase
    {
        public RecordTypeFieldView()
        {
            InitializeComponent();
        }

        protected override Binding GetValidationBinding()
        {
            return BindingOperations.GetBinding(TextBox, TextBox.TextProperty);
        }

        public RecordTypeFieldViewModel ViewModel
        {
            get { return DataContext as RecordTypeFieldViewModel; }
            set { DataContext = value; }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (
                !TextBox.IsKeyboardFocusWithin
                || ViewModel == null
                || !ViewModel.IsEditable
                || ViewModel.AutocompleteViewModel == null
                || !ViewModel.AutocompleteViewModel.AutoSearch)
                return;
            ViewModel.AutocompleteViewModel.SearchText = TextBox.Text;
            ViewModel.AutocompleteViewModel.DynamicGridViewModel.ReloadGrid();
            ViewModel.DisplayAutocomplete = true;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel == null || !ViewModel.IsEditable)
                return;
            if (e.Key == Key.Down)
                ViewModel.SelectAutocompleteGrid();
            if (e.Key == Key.Tab || e.Key == Key.Escape)
                ViewModel.DisplayAutocomplete = false;
            if (e.Key == Key.Enter)
            {
                if (ViewModel.AutocompleteViewModel != null)
                    ViewModel.AutocompleteViewModel.SearchText = TextBox.Text;
                ViewModel.Search();
            }
            else if (e.Key == Key.K &&
                     (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
            {
                if (ViewModel.AutocompleteViewModel != null)
                    ViewModel.AutocompleteViewModel.SearchText = TextBox.Text;
                ViewModel.Search();
            }
        }
    }
}