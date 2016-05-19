#region

using System.Windows.Forms;
using System.Windows.Input;
using JosephM.Application.ViewModel.RecordEntry.Field;
using Binding = System.Windows.Data.Binding;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    public partial class LookupPicklistView : FieldControlBase
    {
        public LookupPicklistView()
        {
            InitializeComponent();
        }

        protected override Binding GetValidationBinding()
        {
            return null;
        }
    }
}