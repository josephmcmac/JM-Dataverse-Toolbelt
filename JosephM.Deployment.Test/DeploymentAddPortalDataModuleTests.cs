using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Deployment.CreatePackage;
using JosephM.Deployment.ExportXml;
using JosephM.Deployment.MigrateRecords;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentAddPortalDataModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void DeploymentAddPortalDataModuleTest()
        {
            PrepareTests();
            var types = new[] { Entities.jmcg_testentitytwo, Entities.jmcg_testentitythree, Entities.jmcg_testentity };

            RecreatePortalData();

            var createApplication = CreateAndLoadTestApplication<CreatePackageModule>();
            createApplication.AddModule<AddPortalDataModule>();

            var entryScreen = createApplication.NavigateToDialogModuleEntryForm<CreatePackageModule, CreatePackageDialog>();

            //the add portal data buttons dont load in this script
            //due to some strange observable collection asyncc things
            //so lets just fake it
            var dataToIncludeField = entryScreen.GetEnumerableFieldViewModel(nameof(CreatePackageRequest.DataToInclude));
            var dataToIncludeGrid = dataToIncludeField.DynamicGridViewModel;
            var customButtons = dataToIncludeGrid.GridsFunctionsToXrmButtons(entryScreen.FormService.GetCustomFunctionsFor(dataToIncludeField.FieldName, entryScreen));
            customButtons.First(b => b.Id == "ADDPORTALDATA").ChildButtons.Last().Invoke();

            var gridRecords = dataToIncludeGrid.GridRecords;

            Assert.IsTrue(gridRecords.Any());
        }
    }
}