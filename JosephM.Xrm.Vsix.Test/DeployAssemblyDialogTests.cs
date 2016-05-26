using System;
using System.IO;
using System.Linq;
using System.Reflection;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Record.Query;
using JosephM.XRM.VSIX.Commands.DeployAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JosephM.Record.Xrm.Test;
using JosephM.Xrm.Test;
using Fields = JosephM.Xrm.Schema.Fields;
using Entities = JosephM.Xrm.Schema.Entities;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class DeployAssemblyDialogTests : JosephMVsixTests
    {
        [TestMethod]
        public void DeployAssemblyDialogTest()
        {
            //todo location  of assemblies etc. sometimes copied to different directory and doesn't locate the TestFiles
            //maybe use the DeploymentItem attribute
            //throw new Exception(Assembly.GetExecutingAssembly().CodeBase + " - " + Assembly.GetExecutingAssembly().Location);

            var pluginAssembly = GetTestPluginAssemblyFile();


            DeleteTestPluginAssembly();

            Assert.IsFalse(GetTestPluginAssemblyRecords().Any());

            var dialog = new DeployAssemblyDialog(CreateDialogController(), pluginAssembly, XrmRecordService);
            dialog.Controller.BeginDialog();

            var objectEntry = (ObjectEntryViewModel)dialog.Controller.UiItems.First();
            objectEntry.OnSave();

            Assert.AreEqual(1, GetTestPluginAssemblyRecords().Count());

            dialog = new DeployAssemblyDialog(CreateDialogController(), pluginAssembly, XrmRecordService);
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
