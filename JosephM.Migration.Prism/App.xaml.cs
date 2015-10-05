using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using JosephM.Migration.Prism.Module;
using JosephM.Prism.Infrastructure.Prism;

namespace JosephM.ParatureMigration
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var prism = new PrismApplication("JosephM Migration");
            prism.AddModule<MigrationModule>();
            prism.Run();
        }
    }
}
