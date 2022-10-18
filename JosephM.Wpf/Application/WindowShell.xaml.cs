using JosephM.Application.Application;
using JosephM.Core.Extentions;
using JosephM.Wpf.Extentions;
using System.Windows;
using System.Windows.Controls;

namespace JosephM.Wpf.Application
{
    /// <summary>
    ///     Interaction logic for WindowShell.xaml
    /// </summary>
    public partial class WindowShell : UserControl
    {
        public WindowShell()
        {
            InitializeComponent();

            DataContextChanged += WindowShell_DataContextChanged;
        }

        private void WindowShell_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is IApplicationController appController
                && appController.AppImageType != null)
            {
                var appImageUserControl = (UserControl)appController.AppImageType.CreateFromParameterlessConstructor();
                appImageUserControl.Height = AppIconItemsControl.Height;
                AppIconItemsControl.Items.Add(appImageUserControl);
            }
            this.DoThemeLoading();
        }
    }
}