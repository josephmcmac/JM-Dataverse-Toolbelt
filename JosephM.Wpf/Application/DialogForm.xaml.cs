#region

using System.Windows;
using System.Windows.Controls;
using JosephM.Application.ViewModel.Dialog;

#endregion

namespace JosephM.Wpf.Application
{
    /// <summary>
    ///     Interaction logic for DialogForm.xaml
    /// </summary>
    public partial class DialogForm : UserControl
    {
        public DialogForm()
        {
            InitializeComponent();
        }


        private DialogViewModel Dialog
        {
            get { return (DialogViewModel) DataContext; }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!Dialog.Controller.IsStarted)
                Dialog.Controller.BeginDialog();
        }
    }
}