using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Xrm.CustomisationExporter;

namespace JosephM.Xrm.CustomisationExporterTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var prism = new PrismApplication("Xrm Customisation Exporter Tool");
            prism.AddModule<XrmModuleModule>();
            prism.AddModule<SavedXrmConnectionsModule>();
            prism.AddModule<CustomisationExporterModule>();
            prism.Run();
        }
    }
}
