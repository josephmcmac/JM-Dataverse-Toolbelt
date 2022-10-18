using JosephM.Application.Application;
using JosephM.Core.Extentions;
using JosephM.Wpf.Extentions;
using JosephM.Wpf.Shared;
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
            if (DataContext is ApplicationBase app
                && app.AppImageUserControlType != null)
            {
                var appImageUserControl = (UserControl)app.AppImageUserControlType.CreateFromParameterlessConstructor();
                appImageUserControl.Height = AppIconItemsControl.Height;
                AppIconItemsControl.Items.Add(appImageUserControl);
            }
            this.DoThemeLoading();
        }
    }
}