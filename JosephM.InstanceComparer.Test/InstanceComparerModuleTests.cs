using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Xrm.Schema;
using JosephM.XrmModule.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace JosephM.InstanceComparer.Test
{
    [TestClass]
    public class InstanceComparerModuleTests : XrmModuleTest
    {
        [TestMethod]
        public void InstanceComparerModuleTest()
        {
            DeleteAll(Entities.jmcg_testentity);
            DeleteAll(Entities.account);
            var t1 = CreateTestRecord(Entities.jmcg_testentity, new Dictionary<string, object>
            {
                { Fields.jmcg_testentity_.jmcg_name, "Blah 1" }
            });
            var a1 = CreateTestRecord(Entities.account, new Dictionary<string, object>
            {
                { Fields.account_.name, "Blah 1" }
            });
            XrmService.Associate(Relationships.jmcg_testentity_.jmcg_testentity_account.Name, Fields.jmcg_testentity_.jmcg_testentityid, t1.Id, Fields.account_.accountid, a1.Id);

            RecreatePortalData();

            var request = new InstanceComparerRequest();
            request.ConnectionOne = GetSavedXrmRecordConfiguration();
            request.ConnectionTwo = GetSavedXrmRecordConfiguration();
            request.DataComparisons = new[]
            {
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.jmcg_testentity, Entities.jmcg_testentity)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.account, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_entityform, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_entityformmetadata, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_webfile, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_webpage, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_webpageaccesscontrolrule, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_webrole, Entities.account)},
                new InstanceComparerRequest.InstanceCompareDataCompare() { RecordType = new RecordType(Entities.adx_websitelanguage, Entities.account)},
            };
            foreach(var prop in request.GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(bool))
                    request.SetPropertyValue(prop.Name, true);
            }
            request.DisplaySavedSettingFields = false;

            var application = CreateAndLoadTestApplication<InstanceComparerModule>();
            var response = application.NavigateAndProcessDialog<InstanceComparerModule, InstanceComparerDialog, InstanceComparerResponse>(request);
            Assert.IsFalse(response.HasError);
            Assert.IsFalse(response.AreDifferences);
        }
    }
}
