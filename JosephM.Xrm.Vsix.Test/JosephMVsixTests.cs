using JosephM.Application.Application;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Core.Utility;
using JosephM.ObjectMapping;
using JosephM.Prism.Infrastructure.Test;
using JosephM.Prism.XrmModule.Crud;
using JosephM.Prism.XrmModule.SavedXrmConnections;
using JosephM.Prism.XrmModule.Test;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.Mappers;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.DeployAssembly;
using JosephM.Xrm.Vsix.Module.PackageSettings;
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

        protected override TestApplication CreateAndLoadTestApplication<TModule>(ApplicationControllerBase applicationController = null, ISettingsManager settingsManager = null, bool loadXrmConnection = true)
        {
            return base.CreateAndLoadTestApplication<TModule>(CreateTestVsixApplicationController(), new VsixSettingsManager(VisualStudioService), loadXrmConnection: InitialiseModuleXrmConnection);
        }

        public ApplicationControllerBase CreateTestVsixApplicationController()
        {
            var container = new VsixDependencyContainer();
            var visualStudioService = VisualStudioService;
            container.RegisterInstance(typeof(IVisualStudioService), visualStudioService);
            container.RegisterInstance(typeof(ISettingsManager), new VsixSettingsManager(visualStudioService));
            var applicationController = new FakeVsixApplicationController(container);
            return applicationController;
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
                        _visualStudioService.AddSolutionItem("solution.xrmconnection", GetXrmRecordConfiguration());
                        _visualStudioService.AddSolutionItem("xrmpackage.xrmsettings", GetTestPackageSettings());
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
            return new FakeDialogController(new FakeApplicationController(VsixDependencyContainer.Create(GetTestPackageSettings(), VisualStudioService)));
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
            var createDialog = new DeployAssemblyDialog(new FakeDialogController(new FakeApplicationController()),
                new FakeVisualStudioService(), XrmRecordService, settings);
            createDialog.Controller.BeginDialog();

            var objectEntry = (ObjectEntryViewModel)createDialog.Controller.UiItems.First();
            objectEntry.OnSave();

            Assert.AreEqual(1, GetTestPluginAssemblyRecords().Count());
        }

        public static DirectoryInfo GetSolutionRootFolder()
        {
            var rootFolderName = "XRM-Developer-Tool";
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().CodeBase.Substring(8));
            var directory = fileInfo.Directory;
            while (directory.Name != rootFolderName)
            {
                directory = directory.Parent;
                if (directory == null)
                    throw new NullReferenceException("Could not find solution root folder of name '" + rootFolderName + "' in " + fileInfo.FullName);
            }
            return directory;
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
    }
}
