#region

using System.Windows;
using System.Windows.Controls;

#endregion

namespace JosephM.Wpf.Application
{
    /// <summary>
    ///     Interaction logic for ApplicatiionOptionsView.xaml
    /// </summary>
    public partial class ApplicationHelpView : UserControl
    {
        public ApplicationHelpView()
        {
            InitializeComponent();
        }

        public void OnDataContextChanged(object s, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}