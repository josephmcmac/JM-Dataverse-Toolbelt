using System.Windows;
using JosephM.CustomisationImporter.Prism;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.Xrm.CodeGenerator.Prism;
using JosephM.Xrm.CustomisationExporter;
using JosephM.Xrm.ImportExporter.Prism;
using JosephM.Xrm.OrganisationSettings.Prism;
using JosephM.Xrm.RecordExtract.RecordExtract;
using JosephM.Xrm.RecordExtract.TextSearch;

namespace JosephM.Xrm.DeveloperTool
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var prism = new PrismApplication("JosephM Xrm Developer Tool");
            prism.AddModule<XrmModuleModule>();
            prism.AddModule<SavedXrmConnectionsModule>();
            prism.AddModule<XrmImporterExporterModule>();
            prism.AddModule<XrmCodeGeneratorModule>();
            prism.AddModule<XrmOrganisationSettingsModule>();
            prism.AddModule<XrmTextSearchModule>();
            prism.AddModule<CustomisationExporterModule>();
            prism.AddModule<CustomisationImportModule>();
            prism.Run();
        }
    }
}