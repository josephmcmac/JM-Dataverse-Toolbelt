using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.XRM.VSIX.Commands.DeployWebResource;
using JosephM.XRM.VSIX.Dialogs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Entities = JosephM.Xrm.Schema.Entities;
using Fields = JosephM.Xrm.Schema.Fields;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixDeployWebResourcesTests : JosephMVsixTests
    {
        [TestMethod]
        public void VsixDeployWebResourcesTest()
        {
            var javaScriptFiles = GetJavaScriptFiles();

            DeleteJavaScriptFileRecords();

            Assert.IsFalse(GetJavaScriptFileRecords().Any());

            var packageSettings = GetTestPackageSettings();

            var deployResourcesService = new DeployWebResourcesService(XrmRecordService, packageSettings);

            Assert.IsTrue(javaScriptFiles.Any());
            var request = new DeployWebResourcesRequest()
            {
                Files = javaScriptFiles
            };
            var dialog = new VsixServiceDialog<DeployWebResourcesService, DeployWebResourcesRequest, DeployWebResourcesResponse, DeployWebResourcesResponseItem>(
                deployResourcesService,
                request,
                new DialogController(new FakeApplicationController()));

            dialog.Controller.BeginDialog();

            Assert.AreEqual(GetJavaScriptFiles().Count(), GetJavaScriptFileRecords().Count());

            dialog = new VsixServiceDialog<DeployWebResourcesService, DeployWebResourcesRequest, DeployWebResourcesResponse, DeployWebResourcesResponseItem>(
                deployResourcesService,
                request,
                new DialogController(new FakeApplicationController()));

            dialog.Controller.BeginDialog();

            Assert.AreEqual(GetJavaScriptFiles().Count(), GetJavaScriptFileRecords().Count());
        }

        private void DeleteJavaScriptFileRecords()
        {
            var deployed = GetJavaScriptFileRecords();

            foreach (var item in deployed)
                XrmRecordService.Delete(item);
        }

        private IEnumerable<IRecord> GetJavaScriptFileRecords()
        {
            var deployed = XrmRecordService.RetrieveAllOrClauses(Entities.webresource,
                GetJavaScriptFiles().Select(
                    j => new Condition(Fields.webresource_.name, ConditionType.Equal, new FileInfo(j).Name)));
            return deployed;
        }

        private static string[] GetJavaScriptFiles()
        {
            var javaScriptFiles = Directory.GetFiles(Path.Combine(GetRootFolder().FullName, "TestFiles", "JavaScript"));
            return javaScriptFiles;
        }
    }
}
