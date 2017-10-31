using JosephM.CodeGenerator.Xrm;
using JosephM.CustomisationExporter.Exporter;
using JosephM.CustomisationImporter.Prism;
using JosephM.Deployment;
using JosephM.InstanceComparer;
using JosephM.Prism.Infrastructure.Module.SavedRequests;
using JosephM.Prism.Infrastructure.Prism;
using JosephM.Prism.XrmModule.Crud;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.Xrm;
using JosephM.RecordCounts.Exporter;
using JosephM.Xrm.RecordExtract.RecordExtract;
using System.Windows;

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
            prism.AddModule<DeploymentModule>();
            prism.AddModule<CodeGeneratorModule>();
            prism.AddModule<XrmRecordExtractModule>();
            prism.AddModule<CustomisationExporterModule>();
            prism.AddModule<CustomisationImportModule>();
            prism.AddModule<InstanceComparerModule>();
            prism.AddModule<RecordCountsModule>();
            prism.AddModule<XrmCrudModule>();
            prism.AddModule<SavedRequestModule>();
            prism.Run();
        }
    }
}