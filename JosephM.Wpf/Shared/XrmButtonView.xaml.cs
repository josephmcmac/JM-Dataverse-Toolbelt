#region

using System.Windows.Controls;
using JosephM.Application.ViewModel.Shared;

#endregion

namespace JosephM.Wpf.Shared
{
    /// <summary>
    ///     Interaction logic for XrmButton.xaml
    /// </summary>
    public partial class XrmButton : UserControl
    {
        public XrmButton()
        {
            InitializeComponent();
        }

        public XrmButtonViewModel ViewModel
        {
            get { return DataContext as XrmButtonViewModel; }
            set { DataContext = value; }
        }

        private void Popup_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }
    }
}