#region

using System.Windows;

#endregion

namespace JosephM.Wpf.Application
{
    /// <summary>
    ///     Interaction logic for Shell.xaml
    /// </summary>
    public partial class WindowShellWindow : Window
    {
        public WindowShellWindow()
        {
            InitializeComponent();
        }

        public void SetContent(object content)
        {
            Container.Content.Content = content;
        }
    }
}