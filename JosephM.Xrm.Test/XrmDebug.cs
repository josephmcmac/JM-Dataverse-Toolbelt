using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace JosephM.Xrm.Test
{
    [TestClass]
    public class XrmDebugScript : XrmTest
    {
        [TestMethod]
        public void XrmDebug()
        {
            var blah = XrmService.WhoAmI();
            //var config = new XrmConfiguration()
            //{
            //    AuthenticationProviderType = Microsoft.Xrm.Sdk.Client.AuthenticationProviderType.LiveId,
            //    DiscoveryServiceAddress = "https://disco.crm6.dynamics.com/XRMServices/2011/Discovery.svc",
            //    OrganizationUniqueName = "org5d9bc2c3",
            //    Username = "joseph@mcmac2017.onmicrosoft.com",
            //    Password = ""
            //};
            //var service = new XrmService(config);
        }

        private void DeleteOnlineSampleData()
        {
            //throw new NotImplementedException("need to turn off plugins for actuals and quote line details");

            var sdkStep = XrmService.GetFirst(Schema.Entities.sdkmessageprocessingstep, Schema.Fields.sdkmessageprocessingstep_.name, "bab1a7b6-0259-4334-98f6-e3cd2afe336a");
            XrmService.Deactivate(sdkStep);
            sdkStep = XrmService.GetFirst(Schema.Entities.sdkmessageprocessingstep, Schema.Fields.sdkmessageprocessingstep_.name, "21af5c99-5189-4421-b897-c33d5e671be2");
            XrmService.Deactivate(sdkStep);
            sdkStep = XrmService.GetFirst(Schema.Entities.sdkmessageprocessingstep, Schema.Fields.sdkmessageprocessingstep_.name, "d53a8d81-bbee-4b9e-ac71-09acca7d6042");
            XrmService.Deactivate(sdkStep);
            sdkStep = XrmService.GetFirst(Schema.Entities.sdkmessageprocessingstep, Schema.Fields.sdkmessageprocessingstep_.name, "487de386-972a-4323-9def-1a1044de3a24");
            XrmService.Deactivate(sdkStep);
            sdkStep = XrmService.GetFirst(Schema.Entities.sdkmessageprocessingstep, Schema.Fields.sdkmessageprocessingstep_.name, "b3d9415e-0532-4654-a6a8-c6047beec413");
            XrmService.Deactivate(sdkStep);

            var toDelete = new[]
            {
                Schema.Entities.task,
                Schema.Entities.email,
                Schema.Entities.phonecall,
                Schema.Entities.letter,
                Schema.Entities.appointment,
                Schema.Entities.incident,
                Schema.Entities.invoice,
                Schema.Entities.contractdetail,
                Schema.Entities.contract,
                Schema.Entities.salesorderdetail,
                "msdyn_actual",
                "msdyn_project",
                "msdyn_fact",
                Schema.Entities.entitlement,
                Schema.Entities.quote,
                Schema.Entities.salesorder,
                Schema.Entities.opportunity,
                Schema.Entities.campaign,
                "bookableresourcebooking",
                "msdyn_resourcerequirement",
                "msdyn_workorder",
                "msdyn_customerasset",
                Schema.Entities.account,
                Schema.Entities.contact,
                Schema.Entities.lead,
            };

            foreach (var entity in toDelete)
            {
                var getAll = XrmService.RetrieveAllEntityType(entity);
                if (entity == "msdyn_project")
                {
                    foreach (var item in getAll)
                    {
                        XrmService.Delete(item);
                    }
                }
                else
                {
                    var allDelete = XrmService.DeleteMultiple(getAll);
                    var errors = allDelete.Where(r => r.Fault != null);
                    foreach (var item in errors)
                        throw new Exception(item.Fault.Message);
                }
            }
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