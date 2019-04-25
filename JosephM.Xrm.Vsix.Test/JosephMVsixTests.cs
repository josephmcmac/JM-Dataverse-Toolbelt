using JosephM.Application.Application;
using JosephM.Application.Desktop.Application;
using JosephM.Application.Desktop.Test;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.AppConfig;
using JosephM.Core.Utility;
using JosephM.ObjectMapping;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.Mappers;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.DeployAssembly;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.Crud;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace JosephM.Xrm.Vsix.Test
{
    public class JosephMVsixTests : XrmModuleTest
    {
        public JosephMVsixTests()
        {
            InitialiseModuleXrmConnection = true;
            XrmRecordService.SetFormService(new XrmFormService());
        }

        public bool InitialiseModuleXrmConnection { get; set; }

        protected override TestApplication CreateAndLoadTestApplication<TModule>(ApplicationControllerBase applicationController = null, ISettingsManager settingsManager = null, bool loadXrmConnection = true, bool addSavedConnectionAppConnectionModule = false)
        {
            var container = new VsixDependencyContainer();
            var visualStudioService = VisualStudioService;
            container.RegisterInstance(typeof(IVisualStudioService), visualStudioService);
            var vsixController = new FakeVsixApplicationController(container);
            var vsixSettingsManager = new VsixSettingsManager(VisualStudioService, new DesktopSettingsManager(vsixController));
            container.RegisterInstance(typeof(ISettingsManager), vsixSettingsManager);
            var app = base.CreateAndLoadTestApplication<TModule>(vsixController, vsixSettingsManager, loadXrmConnection: InitialiseModuleXrmConnection, addSavedConnectionAppConnectionModule: addSavedConnectionAppConnectionModule);
            app.AddModule<PackageSettingsAppConnectionModule>();
            return app;
        }

        private FakeVisualStudioService _visualStudioService;
        public FakeVisualStudioService VisualStudioService
        {
            get
            {
                if (_visualStudioService == null)
                {
                    _visualStudioService = new FakeVisualStudioService();
                    FileUtility.DeleteFiles(_visualStudioService.SolutionDirectory);
                    FileUtility.DeleteSubFolders(_visualStudioService.SolutionDirectory);
                    //okay we 
                    if (InitialiseModuleXrmConnection)
                    {
                        _visualStudioService.AddVsixSetting("solution.xrmconnection", GetXrmRecordConfiguration());
                        _visualStudioService.AddVsixSetting("xrmpackage.xrmsettings", GetTestPackageSettings());
                    }
                }
                return _visualStudioService;
            }
        }

        private XrmPackageSettings _testPackageSettings;
        public XrmPackageSettings GetTestPackageSettings()
        {
            if (_testPackageSettings == null)
            {
                var xrmConfiguration = new InterfaceMapperFor<IXrmConfiguration, XrmConfiguration>().Map(XrmConfiguration);
                var xrmRecordConfiguration = new XrmConfigurationMapper().Map(xrmConfiguration);
                var savedConnection = SavedXrmRecordConfiguration.CreateNew(xrmRecordConfiguration);

                var testSolution = ReCreateTestSolution();
                var packageSettings = new XrmPackageSettings();
                PopulateObject(packageSettings);
                packageSettings.AddToSolution = true;
                packageSettings.Solution = testSolution.ToLookup();
                packageSettings.Connections = new SavedXrmRecordConfiguration[] { savedConnection };
                _testPackageSettings = packageSettings;
            }
            return _testPackageSettings;
        }

        public FakeDialogController CreateDialogController()
        {
            var container = new VsixDependencyContainer();
            var dialogController = new FakeDialogController(new FakeApplicationController(container));
            container.RegisterInstance(typeof(ISettingsManager), new VsixSettingsManager(VisualStudioService, new DesktopSettingsManager(dialogController.ApplicationController)));
            container.RegisterInstance(typeof(ISavedXrmConnections), GetTestPackageSettings());
            return dialogController;
        }

        public static string GetTestPluginAssemblyName()
        {
            var file = new FileInfo(GetTestPluginAssemblyFile());
            return file.Name.Substring(0, file.Name.LastIndexOf(".", StringComparison.Ordinal));
        }

        public static string GetTestPluginAssemblyFile()
        {
            var pluginAssembly = Path.Combine(GetSolutionRootFolder().FullName, "SolutionItems", "TestPluginAssemblyBin",
                PluginAssemblyName + ".dll");
            return pluginAssembly;
        }

        public void DeployAssembly(XrmPackageSettings settings)
        {
            var testApplication = CreateAndLoadTestApplication<DeployAssemblyModule>(loadXrmConnection: false);
            var module = testApplication.GetModule<DeployAssemblyModule>();
            module.DialogCommand();
            var dialog = testApplication.GetNavigatedDialog<DeployAssemblyDialog>();
            var objectEntry = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            objectEntry.OnSave();
            //and verify the assembly now deployed
            Assert.AreEqual(1, GetTestPluginAssemblyRecords().Count());
        }

        public static DirectoryInfo GetRootFolder()
        {

            var assemblyLocation = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            var fileInfo = new FileInfo(assemblyLocation);
            var rootFolder = fileInfo.Directory.Parent.Parent;
            return rootFolder;
        }

        public static string PluginAssemblyName
        {
            get { return "TestXrmSolution.Plugins"; }
        }

        public void DeleteTestPluginAssembly()
        {
            var assemblyRecords = GetTestPluginAssemblyRecords();
            foreach (var assembly in assemblyRecords)
            {
                DeletePluginTriggers(assembly);
                XrmRecordService.Delete(assembly);
            }
        }

        public void DeletePluginTriggers(IRecord assemblyRecord)
        {
            var pluginTriggers = GetPluginTriggers(assemblyRecord);
            foreach (var item in pluginTriggers)
            {
                XrmRecordService.Delete(item);
            }
        }

        public IEnumerable<IRecord> GetPluginTriggers(IRecord assemblyRecord)
        {
            var pluginTypes = XrmRecordService.RetrieveAllAndClauses(Entities.plugintype,
                new[] { new Condition(Fields.plugintype_.pluginassemblyid, ConditionType.Equal, assemblyRecord.Id) });
            if (!pluginTypes.Any())
                throw new NullReferenceException("Not Plugin Types Deployed For Assembly");

            return XrmRecordService.RetrieveAllOrClauses(Entities.sdkmessageprocessingstep,
                pluginTypes.Select(
                    pt => new Condition(Fields.sdkmessageprocessingstep_.plugintypeid, ConditionType.Equal, pt.Id)));
        }

        public IEnumerable<IRecord> GetTestPluginAssemblyRecords()
        {
            var assemblyRecords = XrmRecordService.RetrieveAllAndClauses(Schema.Entities.pluginassembly, new[]
            {
                new Condition(Fields.pluginassembly_.name, ConditionType.Equal, PluginAssemblyName)
            });
            return assemblyRecords;
        }

        public static void SubmitEntryForm(DialogViewModel dialog)
        {
            var entryViewModel = GetEntryForm(dialog);
            Assert.IsTrue(entryViewModel.Validate(), entryViewModel.GetValidationSummary());
            entryViewModel.OnSave();
        }

        public static ObjectEntryViewModel GetEntryForm(DialogViewModel dialog)
        {
            return (ObjectEntryViewModel)dialog.Controller.UiItems.First();
        }

        public static IXrmRecordConfiguration HijackForPackageEntryRedirect(TestApplication app)
        {
            //okay adding this here because I added a redirect to connection entry if none is entered
            var xrmRecordService = app.Controller.ResolveType<XrmRecordService>();
            var visualStudioService = app.Controller.ResolveType<IVisualStudioService>();
            //okay this is the service which will get resolve by the dialog - so lets clear out its connection details
            //then the dialog should redirect to entry
            var originalConnection = xrmRecordService.XrmRecordConfiguration;
            xrmRecordService.XrmRecordConfiguration = new XrmRecordConfiguration();
            //lets delete the settings files, then verify they are recreated during the redirect entry
            var solutionItemsFolder = Path.Combine(visualStudioService.SolutionDirectory, visualStudioService.ItemFolderName);
            FileUtility.DeleteFiles(solutionItemsFolder);
            var solutionSettingFiles = FileUtility.GetFiles(solutionItemsFolder);
            Assert.AreEqual(0, solutionSettingFiles.Count());

            return originalConnection;
        }

        public static void VerifyPackageEntryRedirect(IXrmRecordConfiguration originalConnection, DialogViewModel dialog)
        {
            //okay we should have been directed to a connection entry
            var connectionEntryViewModel = dialog.Controller.UiItems[0] as ObjectEntryViewModel;
            var newConnection = connectionEntryViewModel.GetObject() as SavedXrmRecordConfiguration;
            newConnection.AuthenticationProviderType = originalConnection.AuthenticationProviderType;
            newConnection.DiscoveryServiceAddress = originalConnection.DiscoveryServiceAddress;
            newConnection.OrganizationUniqueName = originalConnection.OrganizationUniqueName;
            newConnection.Domain = originalConnection.Domain;
            newConnection.Username = originalConnection.Username;
            newConnection.Password = originalConnection.Password;
            newConnection.Name = "RedirectScriptEntered";
            connectionEntryViewModel.SaveButtonViewModel.Invoke();
            //okay now we should be directed to the xrm package settings entry
            var packageSettingsEntryViewModel = dialog.Controller.UiItems[0] as ObjectEntryViewModel;
            var newPackageSettings = packageSettingsEntryViewModel.GetObject() as XrmPackageSettings;
            newPackageSettings.SolutionObjectPrefix = "FAKEIT";
            packageSettingsEntryViewModel.SaveButtonViewModel.Invoke();

            //lets just verify the connections were saved as well
            var savedConnections = dialog.ApplicationController.ResolveType<ISavedXrmConnections>();
            Assert.IsTrue(savedConnections.Connections.Any(c => c.Name == "RedirectScriptEntered"));
            var appXrmRecordService = dialog.ApplicationController.ResolveType<XrmRecordService>();
            Assert.IsTrue(appXrmRecordService.XrmRecordConfiguration.ToString() == "RedirectScriptEntered");
            //var appXrmRecordConnection = dialog.ApplicationController.ResolveType<IXrmRecordConfiguration>();
            //Assert.IsTrue(appXrmRecordConnection.ToString() == "RedirectScriptEntered");
            var savedSetingsManager = dialog.ApplicationController.ResolveType<ISettingsManager>();
            var savedXrmRecordService = savedSetingsManager.Resolve<XrmPackageSettings>();
            Assert.IsTrue(appXrmRecordService.XrmRecordConfiguration.ToString() == "RedirectScriptEntered");
            var savedXrmRecordConnection = savedSetingsManager.Resolve<XrmRecordConfiguration>();
        }
    }
}
