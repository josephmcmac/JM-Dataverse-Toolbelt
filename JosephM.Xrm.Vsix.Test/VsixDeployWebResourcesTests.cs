using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Xrm.Vsix.Module.DeployWebResource;
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
        /// <summary>
        /// Scripts through the deploy web resources dialog
        /// </summary>
        [TestMethod]
        public void VsixDeployWebResourcesTest()
        {
            //first delete all the test deployed javascriot files
            var javaScriptFiles = GetJavaScriptFiles();
            DeleteJavaScriptFileRecords();
            Assert.IsFalse(GetJavaScriptFileRecords().Any());

            //create an app, deploy and verify created
            VisualStudioService.SetSelectedItems(GetJavaScriptFiles().Select(f => new FakeVisualStudioProjectItem(f)).ToArray());
            var testApplication = CreateAndLoadTestApplication<DeployWebResourceModule>();
            var dialog = testApplication.NavigateToDialog<DeployWebResourceModule, DeployWebResourceDialog>();
            
            Assert.AreEqual(GetJavaScriptFiles().Count(), GetJavaScriptFileRecords().Count());

            //create an app, deploy againb and verify not duplicated
            VisualStudioService.SetSelectedItems(GetJavaScriptFiles().Select(f => new FakeVisualStudioProjectItem(f)).ToArray());
            testApplication = CreateAndLoadTestApplication<DeployWebResourceModule>();
            dialog = testApplication.NavigateToDialog<DeployWebResourceModule, DeployWebResourceDialog>();

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
