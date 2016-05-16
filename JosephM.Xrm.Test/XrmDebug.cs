using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JosephM.Xrm.Test
{
    [TestClass]
    public class XrmDebugScript : XrmTest
    {
        [TestMethod]
        public void XrmDebug()
        {
            //var files =
            //    Directory.GetFiles(
            //        (@"C:\Users\joseph.mcgregor\Documents\Defence Health\Handover\Solution Packages\DHL-ARCHI-UAT\RETENTION"));

            //var join = string.Join(Environment.NewLine, files);

            //FileUtility.WriteToFile(@"C:\Users\joseph.mcgregor\Desktop", "Files.txt", join);
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