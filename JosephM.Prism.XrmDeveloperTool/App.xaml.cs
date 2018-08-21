using JosephM.Application.Desktop.Application;
using JosephM.Application.Desktop.Console;
using JosephM.Application.Desktop.Module.AboutModule;
using JosephM.Application.Desktop.Module.ReleaseCheckModule;
using JosephM.Application.Desktop.Module.SavedRequests;
using JosephM.CodeGenerator.Xrm;
using JosephM.Core.FieldType;
using JosephM.CustomisationExporter.Exporter;
using JosephM.CustomisationImporter;
using JosephM.Deployment;
using JosephM.InstanceComparer;
using JosephM.RecordCounts;
using JosephM.Xrm.RecordExtract.RecordExtract;
using JosephM.Xrm.RecordExtract.TextSearch;
using JosephM.XrmModule.AppConnection;
using JosephM.XrmModule.Crud;
using JosephM.XrmModule.SavedXrmConnections;
using System.Windows;

namespace JosephM.XrmDeveloperTool
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var app = DesktopApplication.Create("JosephM Xrm Developer Tool");
            app.AddModule<SavedXrmConnectionsModule>();
            app.AddModule<DeploymentModule>();
            app.AddModule<CodeGeneratorModule>();
            app.AddModule<XrmRecordExtractModule>();
            app.AddModule<XrmTextSearchModule>();
            app.AddModule<CustomisationExporterModule>();
            app.AddModule<CustomisationImportModule>();
            app.AddModule<InstanceComparerModule>();
            app.AddModule<RecordCountsModule>();
            app.AddModule<XrmCrudModule>();
            app.AddModule<SavedRequestModule>();
            app.AddModule<ConsoleApplicationModule>();
            app.AddModule<XrmDeveloperToolAboutModule>();
            app.AddModule<DevAppReleaseCheckModule>();
            app.AddModule<SavedConnectionAppConnectionModule>();
            app.Run();
        }

        public class XrmDeveloperToolAboutModule : AboutModule
        {
            public override string CodeLink => "https://github.com/josephmcmac/XRM-Developer-Tool";

            public override string CreateIssueLink => "https://github.com/josephmcmac/XRM-Developer-Tool/issues/new";

            public override Url OtherLink => new Url("https://visualstudiogallery.msdn.microsoft.com/28fb85a8-70d8-4621-96c7-54151eac11cf", "Visual Studio Extention");

            public override string AboutDetail =>
                "This application has been created to improve productivity developing, customising, and deploying solutions in Miscrosoft Dynamics CRM (now known as Dynamics 365)\n" +
                "\n" +
                "My visual studio extention JosephM.Xrm.Vsix also shares many of these features, as well as has additional features for deploying custom code. It can be download at the link shown below\n" +
                "\n" +
                "If you use this app and experience issues, or have any suggestions for improvement, create an issue in github with the link shown below and I will look into it when I get a chance";
        }

        public class DevAppReleaseCheckModule : GitHubReleaseCheckModule
        {
            public override string Githubusername { get { return "josephmcmac"; } }

            public override string GithubRepository { get { return "XRM-Developer-Tool"; } }
        }
    }
}