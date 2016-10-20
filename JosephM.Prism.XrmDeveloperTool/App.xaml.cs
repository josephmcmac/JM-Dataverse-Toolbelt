using System.Windows;
using JosephM.CodeGenerator.Xrm;
using JosephM.CustomisationExporter.Exporter;
using JosephM.CustomisationImporter.Prism;
using JosephM.InstanceComparer;
using JosephM.OrganisationSettings;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Xrm.ImportExporter.Prism;
using JosephM.Xrm.RecordExtract.RecordExtract;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.DeveloperTool
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var prism = new PrismApplication("JosephM Xrm Developer Tool");
            prism.AddModule<XrmModuleModule>();
            prism.AddModule<SavedXrmConnectionsModule>();
            prism.AddModule<XrmImporterExporterModule>();
            prism.AddModule<XrmSolutionImporterExporterModule>();
            prism.AddModule<XrmCodeGeneratorModule>();
            prism.AddModule<MaintainOrganisationModule>();
           // prism.AddModule<XrmTextSearchModule>();
            prism.AddModule<CustomisationExporterModule>();
            prism.AddModule<CustomisationImportModule>();
            prism.AddModule<InstanceComparerModule>();
            prism.Run();
        }
    }
}