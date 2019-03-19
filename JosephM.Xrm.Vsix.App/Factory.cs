using JosephM.Application.Desktop.Module.AboutModule;
using JosephM.Application.Desktop.Module.ApplicationInsights;
using JosephM.Application.Desktop.Module.Themes;
using JosephM.CodeGenerator.JavaScriptOptions;
using JosephM.Core.AppConfig;
using JosephM.CustomisationExporter.Exporter;
using JosephM.Deployment;
using JosephM.Deployment.ImportExcel;
using JosephM.Deployment.MigrateRecords;
using JosephM.InstanceComparer;
using JosephM.RecordCounts;
using JosephM.Xrm.Autonumber;
using JosephM.Xrm.RecordExtract.TextSearch;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module;
using JosephM.Xrm.Vsix.Module.AddPortalCode;
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
        public static VsixApplication CreateJosephMXrmVsixApp(IVisualStudioService visualStudioService, IDependencyResolver container, bool isWizardContext = false)
        {
            var app = VsixApplication.Create(visualStudioService, container, "JosephM.Xrm.Vsix", new Guid("43816e6d-4db8-48d6-8bfa-75916cb080f0"), isWizardContext: isWizardContext);

            app.AddModule<OpenWebModule>(0x010B);
            app.AddModule<OpenSolutionModule>(0x010C);
            app.AddModule<OpenAdvancedFindModule>(0x010D);
            app.AddModule<ClearCacheModule>(0x0109);
            app.AddModule<XrmCrudModule>(0x0112);
            app.AddModule<InstanceComparerModule>(0x0111);
            app.AddModule<UpdateAssemblyModule>(0x0105);
            app.AddModule<XrmPackageSettingsModule>(0x0106);
            app.AddModule<DeployAssemblyModule>(0x0103);
            app.AddModule<ManagePluginTriggersModule>(0x0104);
            app.AddModule<VsixCustomisationImportModule>(0x010A);
            app.AddModule<VsixCreatePackageModule>(0x010E);
            app.AddModule<RefreshSchemaModule>(0x0100);
            app.AddModule<DeployWebResourceModule>(0x0102);
            //app.AddModule<VsixImportCsvsModule>(0x0108);
            app.AddModule<VsixDeployPackageModule>(0x0110);
            app.AddModule<XrmTextSearchModule>(0x0116);
            app.AddModule<XrmPackageAboutModule>(0x0113);
            app.AddModule<ImportSolutionModule>(0x0114);
            app.AddModule<ImportRecordsModule>(0x0115);
            app.AddModule<DeployIntoFieldModule>(0x0117);
            app.AddModule<JavaScriptOptionsModule>(0x0118);
            app.AddModule<OpenDefaultSolutionModule>(0x0119);
            app.AddModule<ThemeModule>(0x011A);
            app.AddModule<AddPortalCodeModule>(0x011B);
            app.AddModule<PackageSettingsAppConnectionModule>();
            app.AddModule<MigrateRecordsModule>(0x011C);
            app.AddModule<AutonumberModule>(0x011D);
            app.AddModule<ImportExcelModule>();
            app.AddModule<CustomisationExporterModule>();
            app.AddModule<RecordCountsModule>();
            app.AddModule<AddPortalDataModule>();
            app.AddModule<XrmPackageApplicationInsightsModule>();
            return app;
        }

        public class XrmPackageAboutModule : AboutModule
        {
            public override string CodeLink => "https://github.com/josephmcmac/XRM-Developer-Tool";

            public override string CreateIssueLink => "https://github.com/josephmcmac/XRM-Developer-Tool/issues/new";

            public override Core.FieldType.Url OtherLink => new Core.FieldType.Url("https://github.com/josephmcmac/XRM-Developer-Tool/releases/latest", "Download Desktop App");

            //public override Assembly MainAssembly => Assembly.GetExecutingAssembly();

            public override string AboutDetail =>
                "This extention has been created to improve productivity developing, customising, and deploying solutions in Miscrosoft Dynamics CRM (now know as Dynamics 365)\n" +
                "\n" +
                "My desktop application also shares many of these features, as well includes others. It can be download at the link shown below\n" +
                "\n" +
                "If you use this extention and experience issues, or have any suggestions for improvement, create an issue in github with the link shown below and I will look into it when I get a chance";
        }

        public class XrmPackageApplicationInsightsModule : ApplicationInsightsModule
        {
            public override string InstrumentationKey => "c4de0a87-25be-4678-8585-38caf2b1cfa0";
        }
    }
}
