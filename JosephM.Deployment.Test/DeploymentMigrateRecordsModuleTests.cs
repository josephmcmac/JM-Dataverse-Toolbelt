using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Deployment.ExportXml;
using JosephM.Deployment.MigrateRecords;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace JosephM.Deployment.Test
{
    [TestClass]
    public class DeploymentMigrateRecordsModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void DeploymentMigrateRecordsModuleTest()
        {
            DeleteAll(Entities.account);
            var account = CreateAccount();
            FileUtility.DeleteFiles(TestingFolder);

            var application = CreateAndLoadTestApplication<MigrateRecordsModule>();

            var instance = new MigrateRecordsRequest();
            instance.SourceConnection = GetSavedXrmRecordConfiguration();
            instance.TargetConnection = GetSavedXrmRecordConfiguration();
            instance.IncludeNotes = true;
            instance.IncludeNNRelationshipsBetweenEntities = true;
            instance.RecordTypesToMigrate = new[]
            {
                new ExportRecordType()
                {
                    Type = ExportType.AllRecords,
                    RecordType = new RecordType(Entities.account, Entities.account),
                    IncludeAllFields = true,
                    IncludeOnlyTheseFields = new [] { new FieldSetting() { RecordField = new RecordField(Fields.account_.createdby, Fields.account_.createdby) }},
                    ExplicitValuesToSet = new []
                    {
                        new ExportRecordType.ExplicitFieldValues() { FieldToSet = new RecordField(Fields.account_.address1_line1, Fields.account_.address1_line1), ClearValue = true},
                        new ExportRecordType.ExplicitFieldValues() { FieldToSet = new RecordField(Fields.account_.address1_line2, Fields.account_.address1_line1), ValueToSet = "explicitValue" },
                    }
                }
            };

            var response = application.NavigateAndProcessDialog<MigrateRecordsModule, MigrateRecordsDialog, MigrateRecordsResponse>(instance);
            if(response.HasError)
            {
                Assert.Fail(response.GetResponseItemsWithError().First().Exception.XrmDisplayString());
            }


            var accounts = XrmService.RetrieveAllEntityType(Entities.account);
            foreach (var migratedAccount in accounts)
            {
                Assert.IsNull(migratedAccount.GetField(Fields.account_.address1_line1));
                Assert.AreEqual("explicitValue", migratedAccount.GetField(Fields.account_.address1_line2));
            }
        }

        [TestMethod]
        public void DeploymentMigrateRecordsMatchByNameTest()
        {
            Assert.Inconclusive();
            var migrateName = "MIGRATESCRIPTNAME";
            DeleteAll(Entities.account);
            var account1 = CreateTestRecord(Entities.account);
            account1.SetField(Fields.account_.name, migrateName);
            account1 = UpdateFieldsAndRetreive(account1, Fields.account_.name);
            var account2 = CreateTestRecord(Entities.account);
            account2.SetField(Fields.account_.name, migrateName);
            account2 = UpdateFieldsAndRetreive(account2, Fields.account_.name);

            var altConnection = GetAltSavedXrmRecordConfiguration();
            var altService = new XrmRecordService(altConnection, ServiceFactory);
            var accountsAlt = altService.RetrieveAllAndClauses(Entities.account, null);
            foreach (var account in accountsAlt)
                altService.Delete(account);

            var application = CreateAndLoadTestApplication<MigrateRecordsModule>();

            var instance = new MigrateRecordsRequest();
            instance.SourceConnection = GetSavedXrmRecordConfiguration();
            instance.TargetConnection = GetAltSavedXrmRecordConfiguration();
            instance.IncludeNotes = true;
            instance.IncludeNNRelationshipsBetweenEntities = true;
            instance.MatchByName = false;
            instance.RecordTypesToMigrate = new[]
            {
                new ExportRecordType()
                {
                    Type = ExportType.AllRecords,
                    RecordType = new RecordType(Entities.account, Entities.account),
                    IncludeAllFields = true,
                }
            };

            var response = application.NavigateAndProcessDialog<MigrateRecordsModule, MigrateRecordsDialog, MigrateRecordsResponse>(instance);
            if (response.HasError)
            {
                Assert.Fail(response.GetResponseItemsWithError().First().Exception.XrmDisplayString());
            }
            var summary = response.ImportSummary;
            Assert.AreEqual(1, summary.Count());

            Assert.AreEqual(2, summary.First().Created);

            accountsAlt = altService.RetrieveAllAndClauses(Entities.account, null);
            Assert.AreEqual(2, accountsAlt.Count());

            XrmService.Delete(account1);
            altService.Delete(accountsAlt.First(a => a.Id != account1.Id.ToString()));

            instance = new MigrateRecordsRequest();
            instance.SourceConnection = GetSavedXrmRecordConfiguration();
            instance.TargetConnection = GetAltSavedXrmRecordConfiguration();
            instance.IncludeNotes = true;
            instance.IncludeNNRelationshipsBetweenEntities = true;
            instance.MatchByName = true;
            instance.RecordTypesToMigrate = new[]
            {
                new ExportRecordType()
                {
                    Type = ExportType.AllRecords,
                    RecordType = new RecordType(Entities.account, Entities.account),
                    IncludeAllFields = true,
                }
            };

            response = application.NavigateAndProcessDialog<MigrateRecordsModule, MigrateRecordsDialog, MigrateRecordsResponse>(instance);
            if (response.HasError)
            {
                Assert.Fail(response.GetResponseItemsWithError().First().Exception.XrmDisplayString());
            }

            summary = response.ImportSummary;
            Assert.AreEqual(1, summary.Count());
            Assert.AreEqual(1, summary.First().NoChange);

            accountsAlt = altService.RetrieveAllAndClauses(Entities.account, null);
            Assert.AreEqual(1, accountsAlt.Count());

            DeleteMyToday();
        }
    }
}