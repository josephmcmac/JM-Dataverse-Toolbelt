using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Query;
using JosephM.XRM.VSIX.Commands.DeployAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Entities = JosephM.Xrm.Schema.Entities;
using Fields = JosephM.Xrm.Schema.Fields;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixDeployAssemblyDialogTests : JosephMVsixTests
    {
        [TestMethod]
        public void VsixDeployAssemblyDialogTest()
        {
            //todo location  of assemblies etc. sometimes copied to different directory and doesn't locate the TestFiles
            //maybe use the DeploymentItem attribute
            //throw new Exception(Assembly.GetExecutingAssembly().CodeBase + " - " + Assembly.GetExecutingAssembly().Location);

            var pluginAssembly = GetTestPluginAssemblyFile();
            var packageSettings = GetTestPackageSettings();

            DeleteTestPluginAssembly();

            Assert.IsFalse(GetTestPluginAssemblyRecords().Any());

            var dialog = new DeployAssemblyDialog(CreateDialogController(), pluginAssembly, XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();

            var objectEntry = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            objectEntry.OnSave();

            Assert.AreEqual(1, GetTestPluginAssemblyRecords().Count());

            dialog = new DeployAssemblyDialog(CreateDialogController(), pluginAssembly, XrmRecordService, packageSettings);
            dialog.Controller.BeginDialog();

            objectEntry = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            objectEntry.OnSave();

            Assert.AreEqual(1, GetTestPluginAssemblyRecords().Count());

            var pluginAssemblyRecord = GetTestPluginAssemblyRecords().First();

            var pluginTypes = XrmRecordService.RetrieveAllAndClauses(Entities.plugintype, new[]
            {
                new Condition(Fields.plugintype_.pluginassemblyid, ConditionType.Equal, pluginAssemblyRecord.Id)
            });

            Assert.IsTrue(pluginTypes.Any());

        }
    }
}
