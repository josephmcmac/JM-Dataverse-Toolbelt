#region

using System.Windows.Controls;
using JosephM.Record.Application.Shared;

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
    }
}