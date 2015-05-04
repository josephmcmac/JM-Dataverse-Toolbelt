#region

using System.Windows;
using System.Windows.Controls;

#endregion

namespace JosephM.Wpf.Application
{
    /// <summary>
    ///     Interaction logic for ApplicatiionOptionsView.xaml
    /// </summary>
    public partial class ApplicationOptionsView : UserControl
    {
        public ApplicationOptionsView()
        {
            InitializeComponent();
        }

        public void OnDataContextChanged(object s, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}