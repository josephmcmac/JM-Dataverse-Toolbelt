using JosephM.Xrm.Vsix.Module.DeployIntoField;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Entities = JosephM.Xrm.Schema.Entities;
using Fields = JosephM.Xrm.Schema.Fields;

namespace JosephM.Xrm.Vsix.Test
{
    [TestClass]
    public class VsixDeployIntoFieldTests : JosephMVsixTests
    {
        /// <summary>
        /// Scripts through the deploy into field dialog
        /// </summary>
        [TestMethod]
        public void VsixDeployIntoFieldTest()
        {
            var fieldFilesToImport = GetFilesForFieldImport();
            var fileFilesToImport = GetFilesForFileImport();

            //new target record to deploy into
            var targetRecordName = "TESTDEPLOYINTO";
            var targetRecord = XrmService.GetFirst(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_name, targetRecordName);
            while(targetRecord != null)
            {
                XrmService.Delete(targetRecord);
                targetRecord = XrmService.GetFirst(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_name, targetRecordName);
            }
            targetRecord = CreateTestRecord(Entities.jmcg_testentity, new Dictionary<string, object>
            {
                { Fields.jmcg_testentity_.jmcg_name, targetRecordName }
            });

            //new target record to deploy into attachment
            var targetFileRecordName = "TESTDEPLOYINTO.css";
            var targetFileRecord = XrmService.GetFirst(Entities.adx_webfile, XrmService.GetPrimaryNameField(Entities.adx_webfile), targetFileRecordName);
            while (targetFileRecord != null)
            {
                XrmService.Delete(targetFileRecord);
                targetFileRecord = XrmService.GetFirst(Entities.adx_webfile, XrmService.GetPrimaryNameField(Entities.adx_webfile), targetFileRecordName);
            }
            targetFileRecord = CreateTestRecord(Entities.adx_webfile, new Dictionary<string, object>
            {
                { XrmService.GetPrimaryNameField(Entities.adx_webfile), targetFileRecordName }
            });


            //create an app, deploy and verify
            VisualStudioService.SetSelectedItems(GetFilesForFieldImport().Union(GetFilesForFileImport()).Select(f => new FakeVisualStudioProjectItem(f)).ToArray());

            var testApplication = CreateAndLoadTestApplication<DeployIntoFieldModule>();
            var dialog = testApplication.NavigateToDialog<DeployIntoFieldModule, DeployIntoFieldDialog>();

            //verify target record fields
            targetRecord = Refresh(targetRecord);
            var html = File.ReadAllText(fieldFilesToImport.First(f => f.EndsWith("html")));
            var javascript = File.ReadAllText(fieldFilesToImport.First(f => f.EndsWith("js")));
            var css = File.ReadAllText(fieldFilesToImport.First(f => f.EndsWith("css")));
            Assert.AreEqual(html, targetRecord.GetStringField(Fields.jmcg_testentity_.jmcg_source));
            Assert.AreEqual(javascript, targetRecord.GetStringField(Fields.jmcg_testentity_.jmcg_javascript));
            Assert.AreEqual(css, targetRecord.GetStringField(Fields.jmcg_testentity_.jmcg_css));

            //verify file attachment
            var css2base64 = Convert.ToBase64String(File.ReadAllBytes(fileFilesToImport.First(f => f.EndsWith("css"))));
            var note = GetRegardingNotes(targetFileRecord).First();
            var noteString = note.GetStringField(Fields.annotation_.documentbody);
            Assert.AreEqual(css2base64, noteString);

            //okay lets also update the attachment as this one only created it
            //first set it as something else so when reimported it is changing
            var htmlBase64 = Convert.ToBase64String(File.ReadAllBytes(fieldFilesToImport.First(f => f.EndsWith("html"))));
            note.SetField(Fields.annotation_.documentbody, htmlBase64);
            note = UpdateFieldsAndRetreive(note, new[] { Fields.annotation_.documentbody });

            //create an app, deploy and verify
            testApplication = CreateAndLoadTestApplication<DeployIntoFieldModule>();
            dialog = testApplication.NavigateToDialog<DeployIntoFieldModule, DeployIntoFieldDialog>();

            //verify still 1 note
            var notes = GetRegardingNotes(targetFileRecord);
            Assert.AreEqual(1, notes.Count());
            //and it was correctly updated
            noteString = notes.First().GetStringField(Fields.annotation_.documentbody);
            Assert.AreEqual(css2base64, noteString);
        }

        private static string[] GetFilesForFieldImport()
        {
            var javaScriptFiles = Directory.GetFiles(Path.Combine(GetSolutionRootFolder().FullName, "SolutionItems", "TestFiles", "TEST ENTITY"));
            return javaScriptFiles;
        }

        private static string[] GetFilesForFileImport()
        {
            var javaScriptFiles = Directory.GetFiles(Path.Combine(GetSolutionRootFolder().FullName, "SolutionItems", "TestFiles", "WEB FILE"));
            return javaScriptFiles;
        }

        private IEnumerable<Entity> GetRegardingNotes(Entity entity)
        {
            return XrmService.RetrieveAllAndConditions(Entities.annotation, new[] { new ConditionExpression(Fields.annotation_.objectid, ConditionOperator.Equal, entity.Id) });
        }
    }
}
