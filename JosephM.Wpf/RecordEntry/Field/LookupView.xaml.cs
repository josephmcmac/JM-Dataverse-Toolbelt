#region

using System.Windows.Forms;
using System.Windows.Input;
using JosephM.Application.ViewModel.RecordEntry.Field;
using Binding = System.Windows.Data.Binding;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    public partial class LookupView : FieldControlBase
    {
        public LookupView()
        {
            InitializeComponent();
        }

        public IReferenceFieldViewModel ViewModel
        {
            get { return DataContext as IReferenceFieldViewModel; }
            set { DataContext = value; }
        }

        protected override Binding GetValidationBinding()
        {
            return null;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                ViewModel.Search();
            if (e.Key == Key.Down)
                ViewModel.SelectLookupGrid();
            else if (e.Key == Key.K &&
                     (e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)))
                ViewModel.Search();
        }

        private void EnteredTextBox_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            //for some strange reason when enter pressed to search the textbox loses focus
            //so just put this in there to force focus into textbox again
            if (ViewModel.Searching)
                ((System.Windows.Controls.TextBox)sender).Focus();
        }
    }
}