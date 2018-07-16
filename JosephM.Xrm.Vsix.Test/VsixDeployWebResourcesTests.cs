using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Xrm.Schema;
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

            //okay the code also allows match by display name
            //so I will add this to verify that too
            //basically change ones display name and file name then verify it still matches
            var files = GetJavaScriptFiles();
            var records = GetJavaScriptFileRecords();
            var firstFileInfo = new FileInfo(files.First());
            var record = records.First(e => e.GetStringField(Fields.webresource_.name) == firstFileInfo.Name);
            XrmRecordService.Delete(record);
            var newRecord = XrmRecordService.NewRecord(Entities.webresource);
            newRecord.SetField(Fields.webresource_.name, "jrm_fakescriptname", XrmRecordService);
            newRecord.SetField(Fields.webresource_.displayname, firstFileInfo.Name, XrmRecordService);
            newRecord.SetField(Fields.webresource_.webresourcetype, OptionSets.WebResource.Type.ScriptJScript, XrmRecordService);
            XrmRecordService.Create(newRecord);

            //create an app, deploy again and verify not duplicated
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
                    j => new Condition(Fields.webresource_.displayname, ConditionType.Equal, new FileInfo(j).Name)));
            return deployed;
        }

        private static string[] GetJavaScriptFiles()
        {
            var javaScriptFiles = Directory.GetFiles(Path.Combine(GetSolutionRootFolder().FullName, "SolutionItems", "TestFiles", "JavaScript"));
            return javaScriptFiles;
        }
    }
}
