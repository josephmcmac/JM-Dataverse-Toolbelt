﻿using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Deployment.ImportCsvs;
using JosephM.Prism.XrmModule.Test;
using JosephM.Xrm.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Xrm.ImporterExporter.Test
{
    [TestClass]
    public class DeploymentImportCsvsTests : XrmModuleTest
    {
        [DeploymentItem(@"Files\Account.csv")]
        [DeploymentItem(@"Files\jmcg_testentity_account.csv")]
        [DeploymentItem(@"Files\Test Entity.csv")]
        [DeploymentItem(@"Files\Test Entity Two.csv")]
        [DeploymentItem(@"Files\Test Entity Three.csv")]
        [DeploymentItem(@"Files\Team.csv")]
        [DeploymentItem(@"Files\jmcg_testentity.csv")]
        [TestMethod]
        public void DeploymentExportImportCsvMultipleTest()
        {
            PrepareTests();
            var typesExTeam = new[] { Entities.jmcg_testentitytwo, Entities.jmcg_testentitythree, Entities.jmcg_testentity, Entities.account };
            var workFolder = ClearFilesAndData(typesExTeam);
            var testimportTeam = XrmService.GetFirst("team", "name", "TestImportTeam");
            if (testimportTeam != null)
                XrmService.Delete(testimportTeam);
            DeleteAll(Entities.account);
            File.Copy(@"Account.csv", Path.Combine(workFolder, @"Account.csv"));
            File.Copy(@"jmcg_testentity_account.csv", Path.Combine(workFolder, @"jmcg_testentity_account.csv"));
            File.Copy(@"Test Entity.csv", Path.Combine(workFolder, @"Test Entity.csv"));
            File.Copy(@"Test Entity Two.csv", Path.Combine(workFolder, @"Test Entity Two.csv"));
            File.Copy(@"Test Entity Three.csv", Path.Combine(workFolder, @"Test Entity Three.csv"));
            File.Copy(@"Team.csv", Path.Combine(workFolder, @"Team.csv"));

            var application = CreateAndLoadTestApplication<ImportCsvsModule>();

            var request = new ImportCsvsRequest
            {
                Folder = new Folder(workFolder),
                DateFormat = DateFormat.American
            };

            application.NavigateAndProcessDialog<ImportCsvsModule, ImportCsvsDialog>(request);

            application = CreateAndLoadTestApplication<ImportCsvsModule>();

            request = new ImportCsvsRequest
            {
                Folder = new Folder(workFolder),
                DateFormat = DateFormat.American,
                FolderOrFiles = ImportCsvsRequest.CsvImportOption.SpecificFiles,
                CsvsToImport = new[] { new ImportCsvsRequest.CsvToImport() { Csv = new FileReference(Path.Combine(workFolder, @"Account.csv")) } }
            };
            application.NavigateAndProcessDialog<ImportCsvsModule, ImportCsvsDialog>(request);

            File.Copy(@"jmcg_testentity.csv", Path.Combine(workFolder, @"jmcg_testentity.csv"));

            //this one sets a record inactive state
            //and matches on multiple keys
            var accounta = CreateTestRecord(Entities.account, new Dictionary<string, object>()
            {
                { Fields.account_.name, "accounta" }
            });
            var accountb = CreateTestRecord(Entities.account, new Dictionary<string, object>()
            {
                { Fields.account_.name, "accountb" }
            });
            var testMultiKeya = CreateTestRecord(Entities.jmcg_testentity, new Dictionary<string, object>()
            {
                { Fields.jmcg_testentity_.jmcg_name, "TESTMATCH" },
                { Fields.jmcg_testentity_.jmcg_account, accounta.ToEntityReference() },
            });
            var testMultiKeyb = CreateTestRecord(Entities.jmcg_testentity, new Dictionary<string, object>()
            {
                { Fields.jmcg_testentity_.jmcg_name, "TESTMATCH" },
                { Fields.jmcg_testentity_.jmcg_account, accountb.ToEntityReference() },
            });


            application = CreateAndLoadTestApplication<ImportCsvsModule>();

            request = new ImportCsvsRequest
            {
                Folder = new Folder(workFolder),
                DateFormat = DateFormat.English,
                FolderOrFiles = ImportCsvsRequest.CsvImportOption.SpecificFiles,
                CsvsToImport = new[] { new ImportCsvsRequest.CsvToImport() { Csv = new FileReference(Path.Combine(workFolder, @"jmcg_testentity.csv")) } }
            };

            application.NavigateAndProcessDialog<ImportCsvsModule, ImportCsvsDialog>(request);
            //todo verify no errors

            var entity = XrmService.GetFirst(Entities.jmcg_testentity, Fields.jmcg_testentity_.jmcg_name, "BLAH 2");
            Assert.AreEqual(XrmPicklists.State.Inactive, entity.GetOptionSetValue(Fields.jmcg_testentity_.statecode));
            Assert.AreEqual(XrmPicklists.State.Inactive, Refresh(testMultiKeya).GetOptionSetValue(Fields.jmcg_testentity_.statecode));
            Assert.AreEqual(XrmPicklists.State.Inactive, Refresh(testMultiKeyb).GetOptionSetValue(Fields.jmcg_testentity_.statecode));
        }

        [DeploymentItem(@"Files\Account.csv")]
        [TestMethod]
        public void DeploymentImportCsvsTestMaskEmails()
        {
            PrepareTests();
            var workFolder = ClearFilesAndData();
            DeleteAll(Entities.account);
            File.Copy(@"Account.csv", Path.Combine(workFolder, @"Account.csv"));

            var importerExporterService = new ImportCsvsService(XrmRecordService);

            var request = new ImportCsvsRequest
            {
                Folder = new Folder(workFolder),
                FolderOrFiles = ImportCsvsRequest.CsvImportOption.SpecificFiles,
                CsvsToImport = new[] { new ImportCsvsRequest.CsvToImport() { Csv = new FileReference(Path.Combine(workFolder, @"Account.csv")) } },
                MaskEmails = true

            };
            var response = importerExporterService.Execute(request, Controller);
            if (response.HasError)
                Assert.Fail(response.GetResponseItemsWithError().First().Exception.DisplayString());

            var entity = XrmService.GetFirst(Entities.account);
            Assert.IsTrue(entity.GetStringField(Fields.account_.emailaddress1).Contains("_AT_"));

            request = new ImportCsvsRequest
            {
                Folder = new Folder(workFolder),
                FolderOrFiles = ImportCsvsRequest.CsvImportOption.SpecificFiles,
                CsvsToImport = new[] { new ImportCsvsRequest.CsvToImport() { Csv = new FileReference(Path.Combine(workFolder, @"Account.csv")) } },
                MaskEmails = false
            };
            response = importerExporterService.Execute(request, Controller);
            if (response.HasError)
                Assert.Fail(response.GetResponseItemsWithError().First().Exception.DisplayString());

            entity = XrmService.GetFirst(Entities.account);
            Assert.IsFalse(entity.GetStringField(Fields.account_.emailaddress1).Contains("_AT_"));
        }

        private string ClearFilesAndData(params string[] typesToDelete)
        {
            foreach (var type in typesToDelete)
                DeleteAll(type);
            var workFolder = WorkFolder;

            FileUtility.DeleteFiles(workFolder);
            return workFolder;
        }

        private string WorkFolder
        {
            get
            {
                var workFolder = TestingFolder + @"\ExportedRecords";
                if (!Directory.Exists(workFolder))
                    Directory.CreateDirectory(workFolder);
                return workFolder;
            }
        }
    }
}