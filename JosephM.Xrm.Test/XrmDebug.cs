﻿using JosephM.Xrm.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.Linq;

namespace JosephM.Xrm.Test
{
    //test brahim
    [TestClass]
    public class XrmDebugScript : XrmTest
    {
        [TestMethod]
        public void XrmDebug()
        {
            //var entityMetadata = XrmService.GetEntityMetadata("aaduser");

            var me = XrmService.WhoAmI();

            //DeleteOnlineSampleData();

            //var query = new Microsoft.Xrm.Sdk.Query.QueryExpression(Entities.activitymimeattachment);
            //var activityJoin = query.AddLink(Entities.email, Fields.activitymimeattachment_.activityid, Fields.emailhash_.activityid);

            //var top1 = XrmService.RetrieveFirst(query);
        }

        private void DeleteOnlineSampleData()
        {
            //throw new NotImplementedException("need to turn off plugins for actuals and quote line details");

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
                if (XrmService.EntityExists(entity))
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