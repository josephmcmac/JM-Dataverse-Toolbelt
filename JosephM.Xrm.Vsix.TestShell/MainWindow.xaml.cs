using JosephM.Application.Options;
using System;
using System.Windows;
using JosephM.Wpf.Extentions;

namespace JosephM.Xrm.Vsix.TestShell
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContextChanged += WindowShell_DataContextChanged;
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            var theApplication = System.Windows.Application.Current as App;
            var myApplication = theApplication.VsixApplication;

            headingView.DataContext = myApplication.Controller;

            var applicationOptions = myApplication.Controller.ResolveType(typeof(IApplicationOptions)) as IApplicationOptions;
            if (applicationOptions == null)
                throw new NullReferenceException("applicationOptions");
            optionsView.DataContext = applicationOptions;
            DataContext = myApplication;

        }

        private void WindowShell_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //in the actual apps this loads in a user control bound to the application controller
            //soi I will wire the method via the heading control which is bound to it
            headingView.DoThemeLoading();
        }
    }
}
