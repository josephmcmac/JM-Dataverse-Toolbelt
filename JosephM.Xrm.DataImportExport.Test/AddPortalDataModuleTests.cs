using JosephM.Xrm.DataImportExport.Modules;
using JosephM.Xrm.DataImportExport.XmlImport;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Xrm.DataImportExport.Test
{
    [TestClass]
    public class AddPortalDataModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void AddPortalDataModuleTest()
        {
            PrepareTests();
            var types = new[] { Entities.jmcg_testentitytwo, Entities.jmcg_testentitythree, Entities.jmcg_testentity };

            RecreatePortalData();

            var createApplication = CreateAndLoadTestApplication<ExportXmlModule>();
            createApplication.AddModule<AddPortalDataModule>();

            var entryScreen = createApplication.NavigateToDialogModuleEntryForm<ExportXmlModule, ExportXmlDialog>();

            //the add portal data buttons dont load in this script
            //due to some strange observable collection asyncc things
            //so lets just fake it
            var dataToIncludeField = entryScreen.GetEnumerableFieldViewModel(nameof(ExportXmlRequest.RecordTypesToExport));
            var dataToIncludeGrid = dataToIncludeField.DynamicGridViewModel;
            var customButtons = dataToIncludeGrid.GridsFunctionsToXrmButtons(entryScreen.FormService.GetCustomFunctionsFor(dataToIncludeField.FieldName, entryScreen));
            customButtons.First(b => b.Id == "ADDPORTALDATA").ChildButtons.Last().Invoke();

            var gridRecords = dataToIncludeGrid.GridRecords;

            Assert.IsTrue(gridRecords.Any());
        }
    }
}