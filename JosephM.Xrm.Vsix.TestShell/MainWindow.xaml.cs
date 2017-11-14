using JosephM.Application.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
