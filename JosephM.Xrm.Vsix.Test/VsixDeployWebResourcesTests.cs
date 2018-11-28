using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using JosephM.Xrm.Vsix.Module.DeployWebResource;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JosephM.Core.AppConfig;
using Entities = JosephM.Xrm.Schema.Entities;
using Fields = JosephM.Xrm.Schema.Fields;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.XrmModule.SavedXrmConnections;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.Core.Utility;
using JosephM.Application.Application;

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
            var app = CreateAndLoadTestApplication<DeployWebResourceModule>();

            //run the deploy including a redirect to enter package settings first
            var originalConnection = HijackForPackageEntryRedirect(app);
            //lets delete the settings files, then verify they are recreated during the redirect entry
            var solutionItemsFolder = Path.Combine(VisualStudioService.SolutionDirectory, VisualStudioService.ItemFolderName);
            FileUtility.DeleteFiles(solutionItemsFolder);
            var solutionSettingFiles = FileUtility.GetFiles(solutionItemsFolder);
            Assert.AreEqual(0, solutionSettingFiles.Count());

            //run the dialog
            var dialog = app.NavigateToDialog<DeployWebResourceModule, DeployWebResourceDialog>();
            VerifyPackageEntryRedirect(originalConnection, dialog);
            //verify the 2 settings files recreated
            solutionSettingFiles = FileUtility.GetFiles(solutionItemsFolder);
            Assert.AreEqual(2, solutionSettingFiles.Count());
            Assert.AreEqual(GetJavaScriptFiles().Count(), GetJavaScriptFileRecords().Count());

            var records = GetJavaScriptFileRecords();
            var currentComponentIds = XrmRecordService.GetSolutionComponents(dialog.Service.PackageSettings.Solution.Id, OptionSets.SolutionComponent.ObjectTypeCode.WebResource).ToList();
            Assert.IsTrue(records.All(r => currentComponentIds.Contains(r.Id)));
            //okay the code also allows match by display name
            //so I will add this to verify that too
            //basically change ones display name and file name then verify it still matches
            var files = GetJavaScriptFiles();
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
            app = CreateAndLoadTestApplication<DeployWebResourceModule>();
            dialog = app.NavigateToDialog<DeployWebResourceModule, DeployWebResourceDialog>();

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
