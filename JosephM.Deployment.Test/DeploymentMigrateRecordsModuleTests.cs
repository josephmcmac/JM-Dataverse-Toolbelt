using JosephM.Application.ViewModel.Grid;
using JosephM.Application.ViewModel.Query;
using JosephM.Application.ViewModel.RecordEntry.Field;
using JosephM.Application.ViewModel.RecordEntry.Form;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.FieldType;
using JosephM.Core.Utility;
using JosephM.Deployment.ExportXml;
using JosephM.Deployment.ImportXml;
using JosephM.Deployment.MigrateRecords;
using JosephM.Prism.XrmModule.Test;
using JosephM.Record.Extentions;
using JosephM.Record.Query;
using JosephM.Xrm;
using JosephM.Xrm.Test;
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
            //todo this for migrate records including some explicit values

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
                    IncludeOnlyTheseFieldsInExportedRecords = new [] { new FieldSetting() { RecordField = new RecordField(Fields.account_.createdby, Fields.account_.createdby) }},
                    ExplicitValuesToSet = new []
                    {
                        new ExportRecordType.ExplicitFieldValues() { FieldToSet = new RecordField(Fields.account_.address1_line1, Fields.account_.address1_line1), ClearValue = true},
                        new ExportRecordType.ExplicitFieldValues() { FieldToSet = new RecordField(Fields.account_.address1_line2, Fields.account_.address1_line1), ValueToSet = "explicitValue" },
                    }
                }
            };

            var response = application.NavigateAndProcessDialog<MigrateRecordsModule, MigrateRecordsDialog, MigrateRecordsResponse>(instance);
            Assert.IsFalse(response.HasError);

            var accounts = XrmService.RetrieveAllEntityType(Entities.account);
            foreach (var migratedAccount in accounts)
            {
                Assert.IsNull(migratedAccount.GetField(Fields.account_.address1_line1));
                Assert.AreEqual("explicitValue", migratedAccount.GetField(Fields.account_.address1_line2));
            }
        }
    }
}