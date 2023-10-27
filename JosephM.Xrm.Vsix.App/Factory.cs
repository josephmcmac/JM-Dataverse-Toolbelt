using JosephM.Application.Desktop.Module.AboutModule;
using JosephM.Application.Desktop.Module.ApplicationInsights;
using JosephM.Application.Desktop.Module.OpenLink;
using JosephM.Application.Desktop.Module.Settings;
using JosephM.Application.Desktop.Module.Themes;
using JosephM.CodeGenerator.JavaScriptOptions;
using JosephM.Core.AppConfig;
using JosephM.RecordCounts;
using JosephM.ToolbeltTheme;
using JosephM.Xrm.DataImportExport.Modules;
using JosephM.Xrm.TextSearch;
using JosephM.Xrm.Vsix.App.Module.Web;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module;
using JosephM.Xrm.Vsix.Module.AddPortalCode;
using JosephM.Xrm.Vsix.Module.AddReleaseData;
using JosephM.Xrm.Vsix.Module.CreatePackage;
using JosephM.Xrm.Vsix.Module.CustomisationImport;
using JosephM.Xrm.Vsix.Module.DeployAssembly;
using JosephM.Xrm.Vsix.Module.DeployIntoField;
using JosephM.Xrm.Vsix.Module.DeployPackage;
using JosephM.Xrm.Vsix.Module.DeployWebResource;
using JosephM.Xrm.Vsix.Module.ImportRecords;
using JosephM.Xrm.Vsix.Module.ImportSolution;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.Xrm.Vsix.Module.PluginTriggers;
using JosephM.Xrm.Vsix.Module.RefreshSchema;
using JosephM.Xrm.Vsix.Module.UpdateAssembly;
using JosephM.Xrm.Vsix.Module.Web;
using JosephM.XrmModule.Crud;
using System;

namespace JosephM.Xrm.Vsix.App
{
    public static class Factory
    {
        public static VsixApplication CreateJosephMXrmVsixApp(IVisualStudioService visualStudioService, IDependencyResolver container, bool isNonSolutionExplorerContext = false, string appName = null)
        {
            var app = VsixApplication.Create(visualStudioService, container, appName ?? "JM Dataverse Toolbelt Dev Kit", new Guid("43816e6d-4db8-48d6-8bfa-75916cb080f0"), isNonSolutionExplorerContext: isNonSolutionExplorerContext);

            app.AddModule<ToolbeltThemeModule>();
            app.AddModule<OpenWebModule>(0x010B);
            app.AddModule<OpenSolutionModule>(0x010C);
            app.AddModule<OpenAdvancedFindModule>(0x010D);
            app.AddModule<ClearCacheModule>(0x0109);
            app.AddModule<XrmCrudModule>(0x0112);
            app.AddModule<UpdateAssemblyModule>(0x0105);
            app.AddModule<XrmPackageSettingsModule>(0x0106);
            app.AddModule<DeployAssemblyModule>(0x0103);
            app.AddModule<ManagePluginTriggersModule>(0x0104);
            app.AddModule<VsixCustomisationImportModule>(0x010A);
            app.AddModule<VsixCreatePackageModule>(0x010E);
            app.AddModule<RefreshSchemaModule>(0x0100);
            app.AddModule<DeployWebResourceModule>(0x0102);
            app.AddModule<VsixDeployPackageModule>(0x0110);
            app.AddModule<AddReleaseDataModule>(0x0121);
            app.AddModule<XrmTextSearchModule>(0x0116);
            app.AddModule<XrmPackageAboutModule>(0x0113);
            app.AddModule<VsixImportSolutionModule>(0x0114);
            app.AddModule<ImportRecordsModule>(0x0115);
            app.AddModule<DeployIntoFieldModule>(0x0117);
            app.AddModule<JavaScriptOptionsModule>(0x0118);
            app.AddModule<OpenDefaultSolutionModule>(0x0119);
            app.AddModule<OpenSettingsModule>(0x0120);
            app.AddModule<SettingsAggregatorModule>(0x011E);
            app.AddModule<AddPortalCodeModule>(0x011B);
            app.AddModule<PackageSettingsAppConnectionModule>();
            app.AddModule<DonateModule>(0x0220);
            app.AddModule<RecordCountsModule>();
            app.AddModule<ColourThemeModule>();
            app.AddModule<XrmPackageApplicationInsightsModule>();
            app.AddModule<ExportDataUsabilityModule>();
            app.AddModule<OpenWebSettingsModule>();
            return app;
        }

        public class XrmPackageAboutModule : AboutModule
        {
            public override string CodeLink => "https://github.com/josephmcmac/XRM-Developer-Tool";

            public override string CreateIssueLink => "https://github.com/josephmcmac/XRM-Developer-Tool/issues/new";

            public override Core.FieldType.Url OtherLink => new Core.FieldType.Url("https://github.com/josephmcmac/XRM-Developer-Tool/releases/latest", "Download Desktop App");

            public override string AboutDetail =>
                "This extension is used to improve productivity deploying custom code to the Microsoft Dynamics for CE and Dataverse platform\n" +
                "\n" +
                "My desktop app has additional features for building and administering solutions on the platform and can be download at the link shown below\n" +
                "\n" +
                "If you experience issues or have suggestions for improvement, create an issue in github with the link shown below";
        }

        public class XrmPackageApplicationInsightsModule : ApplicationInsightsModule
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
