using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Xrm.Test
{
    [TestClass]
    public class XrmDebugScript : XrmTest
    {
        [TestMethod]
        public void XrmDebug()
        {
            //var fields = XrmService.GetPrimaryNameField("plugintype");
        }

        private void CreateSomeRecords()
        {
            var primaryField = XrmService.GetPrimaryNameField("jmcg_testentity");
            var testEntity = CreateRecordAllFieldsPopulated("jmcg_testentity");
            testEntity.SetField(primaryField, "Test Entity 1");
            testEntity = UpdateFieldsAndRetreive(testEntity, primaryField);
            testEntity = CreateRecordAllFieldsPopulated("jmcg_testentity");
            testEntity.SetField(primaryField, "Test Entity 2");
            testEntity = UpdateFieldsAndRetreive(testEntity, primaryField);
            testEntity = CreateRecordAllFieldsPopulated("jmcg_testentity");
            testEntity.SetField(primaryField, "Test Entity 3");
            testEntity = UpdateFieldsAndRetreive(testEntity, primaryField);
            testEntity = CreateRecordAllFieldsPopulated("jmcg_testentity");
            testEntity.SetField(primaryField, "Test Entity 4");
            testEntity = UpdateFieldsAndRetreive(testEntity, primaryField);
        }
    }
}