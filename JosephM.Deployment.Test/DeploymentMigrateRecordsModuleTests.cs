using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Deployment.ExportXml;
using JosephM.Deployment.MigrateRecords;
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
    }
}