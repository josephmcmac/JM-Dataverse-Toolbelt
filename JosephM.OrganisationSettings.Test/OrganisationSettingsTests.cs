using JosephM.Prism.XrmModule.Test;
using JosephM.Xrm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.OrganisationSettings.Test
{
    [TestClass]
    public class OrganisationSettingsTests : XrmModuleTest
    {
        [TestMethod]
        public void OrganisationSettingsTest()
        {
            var testApplication = CreateAndLoadTestApplication<MaintainOrganisationModule>();
            var module = testApplication.GetModule<MaintainOrganisationModule>();
            module.DialogCommand();

            //set to 100 and verify
            var dialog = testApplication.GetNavigatedDialog<MaintainOrganisationDialog>();
            var entryForm = dialog.EntryViewModel;
            var excelLimit = entryForm.GetFieldViewModel("maxrecordsforexporttoexcel");
            excelLimit.ValueObject = 100;
            entryForm.OnSave.Invoke();

            var settings = XrmService.GetFirst("organization");
            Assert.AreEqual(100, settings.GetInt("maxrecordsforexporttoexcel"));


            //set to 10000 and verify
            dialog = testApplication.GetNavigatedDialog<MaintainOrganisationDialog>();
            entryForm = dialog.EntryViewModel;
            excelLimit = entryForm.GetFieldViewModel("maxrecordsforexporttoexcel");
            excelLimit.ValueObject = 10000;
            entryForm.OnSave.Invoke();

            settings = XrmService.GetFirst("organization");
            Assert.AreEqual(10000, settings.GetInt("maxrecordsforexporttoexcel"));
        }
    }
}