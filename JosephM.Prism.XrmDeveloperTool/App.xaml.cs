using JosephM.Application.Prism.Module.AboutModule;
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
using System;
using JosephM.Core.FieldType;

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
            prism.AddModule<XrmDeveloperToolAboutModule>();
            prism.Run();
        }

        public class XrmDeveloperToolAboutModule : AboutModule
        {
            public override string CodeLink => "https://github.com/josephmcmac/XRM-Developer-Tool";

            public override string CreateIssueLink => "https://github.com/josephmcmac/XRM-Developer-Tool/issues/new";

            public override Url OtherLink => new Url("https://visualstudiogallery.msdn.microsoft.com/28fb85a8-70d8-4621-96c7-54151eac11cf", "Visual Studio Extention");

            public override string AboutDetail =>
                "This application has been created to improve productivity developing, customising, and deploying solutions in Miscrosoft Dynamics CRM (now know as Dynamics 365)\n" +
                "\n" +
                "My visual studio extention JosephM.Xrm.Vsix also shares many of these features, as well as has additional features for deploying custom code. It can be download at the link shown below\n" +
                "\n" +
                "If you use this app and experience issues, or have any suggestions for improvement, create an issue in github with the link shown below and I will look into it when I get a chance";
        }
    }
}