#region

using JosephM.Application.ViewModel.RecordEntry.Field;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Binding = System.Windows.Data.Binding;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    public partial class MultiSelectDialogView : UserControl
    {
        public MultiSelectDialogView()
        {
            InitializeComponent();
        }
    }
}