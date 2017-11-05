#region

using JosephM.Application.ViewModel.RecordEntry.Field;
using System.Windows.Input;
using Binding = System.Windows.Data.Binding;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

#endregion

namespace JosephM.Wpf.RecordEntry.Field
{
    public partial class MultiSelectView : FieldControlBase
    {
        public MultiSelectView()
        {
            InitializeComponent();
        }

        private IMultiSelectFieldViewModel IMultiSelectFieldViewModel
        {
            get
            {
                return DataContext as IMultiSelectFieldViewModel;
            }
        }

        protected override Binding GetValidationBinding()
        {
            return null;
        }

        private void DisplayTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IMultiSelectFieldViewModel != null)
            {
                IMultiSelectFieldViewModel.MultiSelectsVisible = !IMultiSelectFieldViewModel.MultiSelectsVisible;
            }
        }
    }
}