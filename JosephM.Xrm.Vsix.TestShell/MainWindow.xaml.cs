using JosephM.Application.Options;
using System;
using System.Windows;

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


        }
    }
}
