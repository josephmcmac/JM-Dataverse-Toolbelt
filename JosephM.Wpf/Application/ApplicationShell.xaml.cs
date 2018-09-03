using JosephM.Wpf.Extentions;
using System.Windows;
using System.Windows.Controls;

namespace JosephM.Wpf.Application
{
    /// <summary>
    ///     Interaction logic for ApplicationShell.xaml
    /// </summary>
    public partial class ApplicationShell : UserControl
    {
        public ApplicationShell()
        {
            InitializeComponent();

            DataContextChanged += ApplicationShell_DataContextChanged;
        }

        private void ApplicationShell_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.DoThemeLoading();
        }
    }
}