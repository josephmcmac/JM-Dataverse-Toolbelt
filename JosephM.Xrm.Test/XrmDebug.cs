using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;

namespace JosephM.Xrm.Test
{
    [TestClass]
    public class XrmDebugScript : XrmTest
    {
        [TestMethod]
        public void XrmDebug()
        {
            //Schema.Fields.savedquery_.

            var recordType = "opportunity";
            var schemaName = "jmc_abc";
            var indexOf_ = schemaName.IndexOf("_");
            if (indexOf_ == -1)
                throw new Exception("Could not determine prefix of field for new relationship name");
            var prefix = schemaName.Substring(0, indexOf_ + 1);
            var usePrefix = !recordType.StartsWith(prefix);
            //var type = Entities.account;
            //var numberToCreate = 6000;
            //CreateRecords(type, numberToCreate);
        }

        private void CreateRecords(string type, int numberToCreate)
        {
            var setSize = 100;
            var numberCreated = 0;
            while (numberCreated < numberToCreate)
            {
                var creates = new System.Collections.Generic.List<Entity>();
                for (var i = 0; i < setSize; i++)
                {
                    var entity = new Entity(type);
                    entity.SetField(XrmService.GetPrimaryNameField(type), "Test Record " + i);
                    creates.Add(entity);
                    numberCreated++;
                    if (numberCreated >= numberToCreate)
                        break;
                }
                XrmService.CreateMultiple(creates);
            }
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