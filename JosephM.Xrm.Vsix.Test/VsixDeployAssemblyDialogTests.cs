using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Query;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.DeployAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixDeployAssemblyDialogTests : JosephMVsixTests
    {
        /// <summary>
        /// verifies deploy assembly process
        /// </summary>
        [TestMethod]
        public void VsixDeployAssemblyDialogTest()
        {
            //delet the plugin assembly so we verify it gets deployed
            DeleteTestPluginAssembly();
            Assert.IsFalse(GetTestPluginAssemblyRecords().Any());

            //test app do the deploy assembly dialog
            var testApplication = CreateAndLoadTestApplication<DeployAssemblyModule>(loadXrmConnection: false);
            var module = testApplication.GetModule<DeployAssemblyModule>();
            module.DialogCommand();
            var dialog = testApplication.GetNavigatedDialog<DeployAssemblyDialog>();
            var objectEntry = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            objectEntry.OnSave();
            //and verify the assembly now deployed
            Assert.AreEqual(1, GetTestPluginAssemblyRecords().Count());

            //test app do the deploy assembly again
            module = testApplication.GetModule<DeployAssemblyModule>();
            module.DialogCommand();
            dialog = testApplication.GetNavigatedDialog<DeployAssemblyDialog>();
            objectEntry = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            objectEntry.OnSave();
            //verify still one assembly deployed
            Assert.AreEqual(1, GetTestPluginAssemblyRecords().Count());

            //verify plugin type records were created for the assmbly
            var pluginAssemblyRecord = GetTestPluginAssemblyRecords().First();
            var pluginTypes = XrmRecordService.RetrieveAllAndClauses(Entities.plugintype, new[]
            {
                new Condition(Fields.plugintype_.pluginassemblyid, ConditionType.Equal, pluginAssemblyRecord.Id)
            });
            Assert.IsTrue(pluginTypes.Any());
        }

        /// <summary>
        /// verifies for deploy assembly process goes straight to completion screen with message if failed build
        /// </summary>
        [TestMethod]
        public void VsixDeployAssemblyDialogWhereBuildFailedTest()
        {
            var testApplication = CreateAndLoadTestApplication<DeployAssemblyModule>(loadXrmConnection: false);

            //this fakes a failed build as there is no assembly returned when build and get assembly called
            VisualStudioService.SetSelectedProjectAssembly(null);

            //navigate to deploy assembly
            var module = testApplication.GetModule<DeployAssemblyModule>();
            module.DialogCommand();
            var dialog = testApplication.GetNavigatedDialog<DeployAssemblyDialog>();

            //verify straight to completion screen
            var completionScreen = dialog.Controller.UiItems.First() as CompletionScreenViewModel;
            Assert.IsNotNull(completionScreen);
            Assert.IsNotNull(completionScreen.CompletionDetails);
            completionScreen.CompletionDetails.LoadFormSections();

            Assert.IsNotNull(completionScreen.CompletionDetails.GetStringFieldFieldViewModel(nameof(DeployAssemblyResponse.Message)).Value);
        }
    }
}
