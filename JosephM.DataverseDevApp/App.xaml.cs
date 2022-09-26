using JosephM.Application.Desktop.Application;
using JosephM.Application.Desktop.Console;
using JosephM.Application.Desktop.Module.AboutModule;
using JosephM.Application.Desktop.Module.ApplicationInsights;
using JosephM.Application.Desktop.Module.OpenLink;
using JosephM.Application.Desktop.Module.ReleaseCheckModule;
using JosephM.Application.Desktop.Module.SavedRequests;
using JosephM.Application.Desktop.Module.Themes;
using JosephM.Core.FieldType;
using JosephM.CustomisationExporter;
using JosephM.CustomisationImporter;
using JosephM.Deployment;
using JosephM.InstanceComparer;
using JosephM.RecordCounts;
using JosephM.Xrm.Autonumber;
using JosephM.Xrm.DataImportExport.Modules;
using JosephM.Xrm.ExcelImport;
using JosephM.Xrm.MigrateInternal;
using JosephM.Xrm.MigrateRecords;
using JosephM.Xrm.SqlImport;
using JosephM.Xrm.TextSearch;
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

            var app = DesktopApplication.Create("JosephM Dataverse Dev App");
            app.AddModule<SavedXrmConnectionsModule>();
            app.AddModule<DevAppApplicationInsightsModule>();
            app.AddModule<DeploymentModule>();
            app.AddModule<ExportDataUsabilityModule>();
            app.AddModule<ExcelImportModule>();
            app.AddModule<SqlImportModule>();
            app.AddModule<MigrateInternalModule>();
            app.AddModule<MigrateRecordsModule>(); 
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
            app.AddModule<AutonumberModule>();
            app.AddModule<ThemeModule>();
            app.AddModule<DevAppApplicationInsightsModule>();
            app.AddModule<DonateModule>();
            app.Run();
        }

        public class XrmDeveloperToolAboutModule : AboutModule
        {
            public override string CodeLink => "https://github.com/josephmcmac/XRM-Developer-Tool";

            public override string CreateIssueLink => "https://github.com/josephmcmac/XRM-Developer-Tool/issues/new";

            public override Url OtherLink => new Url("https://visualstudiogallery.msdn.microsoft.com/28fb85a8-70d8-4621-96c7-54151eac11cf", "Visual Studio Extention");

            public override string AboutDetail =>
                "This application has been created to improve productivity developing, customising, and deploying solutions in Microsoft Dynamics for CE and PowerApps Common Data Service\n" +
                "\n" +
                "My visual studio extension JosephM.Xrm.Vsix shares many of these features, as well as includes others for deploying code into the platform. It can be download at the link shown below\n" +
                "\n" +
                "If you use this app and experience issues, or have suggestions for improvement, create an issue in github with the link shown below and I will look into it when I get a chance";
        }

        public class DevAppReleaseCheckModule : GitHubReleaseCheckModule
        {
            public override string Githubusername { get { return "josephmcmac"; } }

            public override string GithubRepository { get { return "XRM-Developer-Tool"; } }
        }

        public class DevAppApplicationInsightsModule : ApplicationInsightsModule
        {
            public override string InstrumentationKey => "c4de0a87-25be-4678-8585-38caf2b1cfa0";
        }

        public class DonateModule : OpenLinkModule
        {
            public override int SettingsOrder => 9999999;
            public override string MainOperationName => "Donate";
            public override string UrlToOpen => "https://github.com/sponsors/josephmcmac?frequency=one-time";
        }
    }
}