﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using JosephM.Core.Log;
using JosephM.Core.Test;
using Microsoft.Xrm.Sdk.Query;
using JosephM.Xrm.Schema;
using JosephM.Core.Serialisation;
using JosephM.Core.Extentions;

namespace JosephM.Xrm.Test
{
    public abstract class XrmTest : CoreTest
    {
        protected virtual string OverrideOrganisation
        {
            get
            {
                return null;
            }
        }

        public void PrepareTests(bool testComponentsRequired = false)
        {
            var verifyConnection = XrmService.VerifyConnection();
            if (!verifyConnection.IsValid)
                Assert.Inconclusive("Could Not Connect To Crm Instance To Execute Tests {0}",
                    verifyConnection.GetErrorString());

            if (testComponentsRequired)
            {
                var testComponentSolutionName = "TestComponents";
                var solution = XrmService.GetFirst("solution", "uniquename", testComponentSolutionName);
                if (solution == null)
                    throw new NullReferenceException(
                        string.Format(
                            "Required solution {0} located in Solution Items is not installed in the CRM instance. You will need to install it and rerun the test",
                            testComponentSolutionName));
            }
        }

        private XrmService _xrmService;

        public XrmService XrmService
        {
            get
            {
                if(_xrmService == null)
                    _xrmService = new XrmService(XrmConfiguration, Controller, null);
                return _xrmService;
            }
        }

        protected IUserInterface UI { get; private set; }

        public virtual IXrmConfiguration XrmConfiguration
        {
            get
            {
                var thisAssemblyLocation = Assembly.GetExecutingAssembly().CodeBase;
                var fileInfo = new FileInfo(thisAssemblyLocation.Substring(8));
                var carryDirectory = fileInfo.Directory;
                while (carryDirectory.Parent != null)
                {
                    if (carryDirectory
                        .Parent
                        .GetDirectories()
                        .Any(d => d.Name == "Xrm.Vsix"))
                    {
                        carryDirectory = carryDirectory.Parent;
                        break;
                    }
                    carryDirectory = carryDirectory.Parent;
                }
                if (carryDirectory.Parent == null)
                    throw new Exception("Error resolving connection file");
                var fileName = Path.Combine(carryDirectory.FullName, "Xrm.Vsix", Environment.UserName + ".solution.xrmconnection");
                var readEncryptedConfig = File.ReadAllText(fileName);
                var dictionary =
                        (Dictionary<string, string>)
                            JsonHelper.JsonStringToObject(readEncryptedConfig, typeof(Dictionary<string, string>));

                var xrmConfig = new XrmConfiguration();
                foreach (var prop in xrmConfig.GetType().GetReadWriteProperties())
                {
                    var value = dictionary[prop.Name];
                    if (value != null && prop.Name == nameof(XrmConfiguration.Password))
                        xrmConfig.SetPropertyByString(prop.Name, new JosephM.Core.FieldType.Password(value).GetRawPassword());
                    else
                        xrmConfig.SetPropertyByString(prop.Name, value);
                }
                return xrmConfig;
            }
        }

        public string ExecutionPath
        {
            get
            {
                var assembly = Assembly.GetExecutingAssembly();
                var location = assembly.Location;
                var name = Assembly.GetExecutingAssembly().GetName().Name;
                var executionPath = location.Substring(0, location.Length - name.Length - 4); // the 4 is .dll..
                return executionPath;
            }
        }

        public void LogLiteral(string message)
        {
            Controller.LogLiteral(message);
        }

        protected void RunTestFunctions(Action[] actions)
        {
            foreach (var item in actions)
            {
                Controller.LogLiteral("*******************************************");
                Controller.LogLiteral("Executing Test " + item.Method.Name);
                item();
                Controller.LogLiteral("Executed Test " + item.Method.Name);
                Controller.LogLiteral("*******************************************");
            }
        }

        public void CheckException(Exception ex)
        {
            if (ex is AssertFailedException)
            {
                Controller.LogLiteral("ASSERT FAILED");
                throw ex;
            }
        }

        public void DeleteAllData()
        {
            foreach (var entityType in EntitiesToDelete)
            {
                DeleteAll(entityType);
            }
        }

        public virtual IEnumerable<string> EntitiesToDelete
        {
            get
            {
                return new[]
                {
                    Entities.jmcg_testentity,
                    "account",
                    "contact"
                };
            }
        }

        public void DeleteAll(string entityType, XrmService serviceToUse = null)
        {
            serviceToUse = serviceToUse ?? XrmService;
            var query = new QueryExpression(entityType);
            var items = serviceToUse.RetrieveAll(query);
            serviceToUse.DeleteMultiple(items);
        }

        public Entity CreateTestRecord(string entityType)
        {
            var entity = new Entity(entityType);
            return CreateAndRetrieve(entity);
        }

        public Entity CreateAndRetrieve(Entity entity, XrmService useService = null)
        {
            useService = useService ?? XrmService;
            var primaryField = useService.GetPrimaryNameField(entity.LogicalName);
            if (!entity.Contains(primaryField))
                entity.SetField(primaryField, "Test Record " + DateTime.Now.ToFileTimeUtc());
            switch (entity.LogicalName)
            {
                case "sla":
                    {
                        if (!entity.Contains("applicablefrom"))
                            entity.SetField("applicablefrom", "?");
                        break;
                    }
            }
            return useService.CreateAndRetreive(entity);
        }

        public Entity UpdateAndRetrieve(Entity entity)
        {
            XrmService.Update(entity);
            return XrmService.Retrieve(entity.LogicalName, entity.Id);
        }

        public Entity Refresh(Entity entity)
        {
            return XrmService.Refresh(entity);
        }

        public string TestEntityType
        {
            get { return Entities.jmcg_testentity; }
        }

        public Entity CreateRecordAllFieldsPopulated(string type, Dictionary<string, object> explicitSetFields = null)
        {
            explicitSetFields = explicitSetFields ?? new Dictionary<string, object>();

            var entity = new Entity(type);
            foreach (var explictiSetField in explicitSetFields)
                entity.SetField(explictiSetField.Key, explictiSetField.Value);

            var fieldsToExlcude = new[] { "traversedpath", "parentarticlecontentid", "rootarticleid", "previousarticlecontentid" };
            var fields = XrmService.GetFields(type)
                .Where(f => !fieldsToExlcude.Contains(f));
            foreach (var field in fields.Where(s => !explicitSetFields.ContainsKey(s)))
            {
                if (XrmService.IsWritable(field, type))
                {
                    var fieldType = XrmService.GetFieldType(field, type);
                    switch (fieldType)
                    {
                        case AttributeTypeCode.BigInt:
                            {
                                entity.SetField(field, 1);
                                break;
                            }
                        case AttributeTypeCode.Boolean:
                            {
                                entity.SetField(field, true);
                                break;
                            }
                        case AttributeTypeCode.CalendarRules:
                            {
                                break;
                            }
                        case AttributeTypeCode.Customer:
                            {
                                entity.SetField(field, CreateAccount());
                                break;
                            }
                        case AttributeTypeCode.DateTime:
                            {
                                var now = DateTime.UtcNow;
                                var noMilliSeconds = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second,
                                    DateTimeKind.Utc);
                                entity.SetField(field, noMilliSeconds);
                                break;
                            }
                        case AttributeTypeCode.Decimal:
                            {
                                entity.SetField(field, (decimal)1);
                                break;
                            }
                        case AttributeTypeCode.Double:
                            {
                                entity.SetField(field, (double)1);
                                break;
                            }
                        case AttributeTypeCode.EntityName:
                            {
                                break;
                            }
                        case AttributeTypeCode.Integer:
                            {
                                var options = XrmService.GetPicklistKeyValues(type, field);
                                if (options != null && options.Any())
                                {
                                    entity.SetField(field, options.First().Key);
                                }
                                else
                                {
                                    entity.SetField(field, 1);
                                }
                                break;
                            }
                        case AttributeTypeCode.Lookup:
                            {
                                var target = XrmService.GetLookupTargetEntity(field, type);
                                var typesToExlcude = new[]
                                {
                                    "equipment", "transactioncurrency", "pricelevel", "service", "systemuser", "incident",
                                    "campaign", "territory", "sla", "bookableresource", "msdyn_taxcode", "languagelocale"
                                };
                                if (!typesToExlcude.Contains(target))
                                    entity.SetField(field, CreateTestRecord(target).ToEntityReference());
                                break;
                            }
                        case AttributeTypeCode.ManagedProperty:
                            {
                                break;
                            }
                        case AttributeTypeCode.Memo:
                            {
                                entity.SetField(field, "blah blah blah \n blah");
                                break;
                            }
                        case AttributeTypeCode.Money:
                            {
                                entity.SetField(field, new Money(1));
                                break;
                            }
                        case AttributeTypeCode.Owner:
                            {
                                entity.SetField(field, new EntityReference("systemuser", CurrentUserId));
                                break;
                            }
                        case AttributeTypeCode.PartyList:
                            {
                                entity.AddActivityParty(field, "systemuser", CurrentUserId);
                                break;
                            }
                        case AttributeTypeCode.Picklist:
                            {
                                entity.SetField(field, new OptionSetValue(XrmService.GetOptions(field, type).First().Key));
                                break;
                            }
                        case AttributeTypeCode.State:
                            {
                                break;
                            }
                        case AttributeTypeCode.Status:
                            {
                                break;
                            }
                        case AttributeTypeCode.String:
                            {
                                entity.SetField(field, PopulateStringValue);
                                break;
                            }
                        case AttributeTypeCode.Uniqueidentifier:
                            {
                                break;
                            }
                        case AttributeTypeCode.Virtual:
                            {
                                break;
                            }
                    }
                }
            }
            return CreateAndRetrieve(entity);
        }

        public string PopulateStringValue
        {
            get
            {
                return "1234";
            }
        }

        public Guid CurrentUserId
        {
            get { return XrmService.WhoAmI(); }
        }

        private Guid? _notCurrentUserId;
        public Guid NotCurrentUserId
        {
            get
            {
                if (!_notCurrentUserId.HasValue)
                {

                    var conditions = new[] { new ConditionExpression("systemuserid", ConditionOperator.NotEqual, CurrentUserId), new ConditionExpression("isdisabled", ConditionOperator.Equal, false), new ConditionExpression("fullname", ConditionOperator.NotEqual, "Support User") };
                    var query = XrmService.BuildQuery("systemuser", new string[0], conditions, null);
                    var roleAssociationLink = query.AddLink(Relationships.systemuser_.systemuserroles_association.EntityName, Fields.systemuser_.systemuserid, Fields.systemuser_.systemuserid);
                    var roleLink = roleAssociationLink.AddLink(Entities.role, Fields.role_.roleid, Fields.role_.roleid);
                    roleLink.LinkCriteria.AddCondition(new ConditionExpression(Fields.role_.name, ConditionOperator.Equal, "System Administrator"));
                    var user = XrmService.RetrieveFirst(query);
                    if (user == null)
                        throw new NullReferenceException("Could not find other system administrator user");
                    _notCurrentUserId = user.Id;
                }
                return _notCurrentUserId.Value;
            }
        }

        protected Entity CreateAccount()
        {
            var entity = new Entity("account");
            entity.SetField("name", "Test Account - X");
            entity.SetField("fax", "0999999999fax");
            entity.SetField("telephone1", "0999999999phone");
            entity.SetField("emailaddress1", "testemail@dev.com");
            if (entity.GetField("address1_line1") == null)
            {
                entity.SetField("address1_line1", "1 Bourke St");
                entity.SetField("address1_city", "Melbourne");
                entity.SetField("address1_stateorprovince", "victoria");
                entity.SetField("address1_postalcode", "3000");
            }
            Entity contact = null;
            if (entity.GetField("primarycontactid") == null)
            {
                contact = CreateContact();
                entity.SetLookupField("primarycontactid", contact);
            }

            var account = CreateAndRetrieve(entity);
            if (contact != null)
            {
                contact.SetLookupField("parentcustomerid", account);
                UpdateFieldsAndRetreive(contact, new[] { "parentcustomerid", "ntaa_usecustomerprimaryaddress" });
            }
            return account;
        }

        public Entity CreateContact()
        {
            var contact = new Entity("contact");
            contact.SetField("firstname", "TESTSCRIPT");
            contact.SetField("lastname", "CONTACT " + DateTime.UtcNow.ToFileTimeUtc().ToString());
            return CreateAndRetrieve(contact);
        }


        public Entity UpdateFieldsAndRetreive(Entity entity, IEnumerable<string> fieldsToUpdate)
        {
            XrmService.Update(entity, fieldsToUpdate);
            return XrmService.Retrieve(entity.LogicalName, entity.Id);
        }

        public Entity UpdateFieldsAndRetreive(Entity entity, params string[] fieldsToUpdate)
        {
            XrmService.Update(entity, fieldsToUpdate);
            return XrmService.Retrieve(entity.LogicalName, entity.Id);
        }

        public void DeleteMyToday()
        {
            foreach (var entity in EntitiesToDelete)
                DeleteMyToday(entity);
        }

        public void DeleteMyToday(string entityType)
        {
            var query = XrmService.BuildQueryActive(entityType, new string[] { },
                new[]
                {
                    new ConditionExpression("createdby", ConditionOperator.Equal, CurrentUserId),
                    new ConditionExpression("createdon", ConditionOperator.Today)
                }, null);
            var entities = XrmService.RetrieveAll(query);
            foreach (var entity in entities)
            {
                try
                {
                    XrmService.Delete(entity);
                }
                catch (Exception)
                {
                }
            }
        }

        public Entity CreateTestRecord(string entityType, Dictionary<string, object> fields, XrmService useService = null)
        {
            var entity = new Entity(entityType);
            foreach (var field in fields)
            {
                entity.SetField(field.Key, field.Value);
            }
            return CreateAndRetrieve(entity, useService: useService);
        }

        public object CreateNewEntityFieldValue(string fieldName, string recordType, Entity currentRecord)
        {
            var currentValueNull = currentRecord.GetField(fieldName) == null;
            var fieldType = XrmService.GetFieldType(fieldName, recordType);
            switch (fieldType)
            {
                case (AttributeTypeCode.String):
                    {
                        return currentValueNull ? "BLAH" : "BLAHBLAH";
                    }
                case (AttributeTypeCode.Memo):
                    {
                        return currentValueNull ? "BLAH\n1" : "BLAHBLAH\n1";
                    }
                case (AttributeTypeCode.DateTime):
                    {
                        return currentValueNull ? new DateTime(1980, 11, 15) : new DateTime(2001, 1, 1);
                    }
                case (AttributeTypeCode.Lookup):
                    {
                        var lookupTargetType = XrmService.GetLookupTargetEntity(fieldName, recordType);
                        if (lookupTargetType == "workflow")
                            return null;
                        Entity referenceRecord = null;
                        if (currentValueNull)
                        {
                            referenceRecord = XrmService.GetFirst(lookupTargetType);
                        }
                        else
                        {
                            var rs = XrmService.GetFirstX(lookupTargetType, 2);
                            if (rs.Any(r => r.Id != currentRecord.GetLookupGuid(fieldName)))
                                referenceRecord = rs.First(r => r.Id != currentRecord.GetLookupGuid(fieldName));
                        }
                        if (referenceRecord == null)
                        {
                            referenceRecord = new Entity(lookupTargetType);
                            if (lookupTargetType == "transactioncurrency")
                            {
                                referenceRecord = new Entity("transactioncurrency");
                                referenceRecord.SetField("currencysymbol", "$");
                                referenceRecord.SetField("exchangerate", (decimal).9);
                                referenceRecord.SetField("currencyname", "US Dollar");
                                referenceRecord.SetField("isocurrencycode", "USD");
                            }
                            else
                                referenceRecord.SetField(XrmService.GetPrimaryNameField(lookupTargetType),
                                    "Test Lookup " + DateTime.Now.ToFileTime());
                            referenceRecord.Id = XrmService.Create(referenceRecord);
                        }
                        return referenceRecord.ToEntityReference();
                    }
                case (AttributeTypeCode.Picklist):
                    {
                        var options = XrmService.GetPicklistKeyValues(recordType, fieldName);
                        var option1 = options.First().Key;
                        var option2 = options.Count() > 1 ? options.ElementAt(1).Key : options.First().Key;
                        if (currentValueNull)
                            return new OptionSetValue(option1);
                        else
                            return currentRecord.GetOptionSetValue(fieldName) == option1 ? new OptionSetValue(option2) : new OptionSetValue(option1);
                    }
                case (AttributeTypeCode.Boolean):
                    {
                        return currentValueNull;
                    }
                case (AttributeTypeCode.Integer):
                    {
                        if (XrmService.GetIntegerFormat(fieldName, recordType) == IntegerFormat.TimeZone)
                        {
                            var timezoneRecords = XrmService.GetFirstX("timezonedefinition", 2);
                            if (timezoneRecords.Count() < 2)
                                throw new Exception("At least 2 Records Required");
                            var option1 = timezoneRecords.ElementAt(0).GetInt("timezonecode");
                            var option2 = timezoneRecords.ElementAt(1).GetInt("timezonecode");
                            if (currentValueNull)
                                return option1;
                            else
                                return currentRecord.GetInt(fieldName) == option1 ? option2 : option1;
                        }
                        else
                            return currentValueNull ? 111 : 222;
                    }
                case (AttributeTypeCode.Decimal):
                    {
                        return currentValueNull ? new Decimal(111) : new decimal(222);
                    }
                case (AttributeTypeCode.Money):
                {
                    return currentValueNull ? new Money() {Value = 111} : new Money() {Value = 222};
                }
                case (AttributeTypeCode.Double):
                    {
                        return currentValueNull ? (double)111 : (double)222;
                    }
                case (AttributeTypeCode.Uniqueidentifier):
                    {
                        return currentValueNull ? Guid.NewGuid() : currentRecord.Id;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException("Unmatched field type " + fieldType);
                    }
            }
        }

        private Entity _testQueue;

        public Entity TestQueue
        {
            get
            {
                if (_testQueue == null)
                {
                    _testQueue = XrmService.GetFirst(Entities.queue, Fields.queue_.name, "TESTQUEUE");
                    if (_testQueue == null)
                    {
                        _testQueue = new Entity(Entities.queue);
                        _testQueue.SetField(Fields.queue_.name, "TESTQUEUE");
                        _testQueue.SetField(Fields.queue_.emailaddress, "testqueue@example.com");
                        _testQueue = CreateAndRetrieve(_testQueue);
                    }
                }
                return _testQueue;
            }
        }

        private Entity _testQueue1;

        public Entity TestQueue1
        {
            get
            {
                if (_testQueue1 == null)
                {
                    _testQueue1 = XrmService.GetFirst(Entities.queue, Fields.queue_.name, "TESTQUEUE1");
                    if (_testQueue1 == null)
                    {
                        _testQueue1 = new Entity(Entities.queue);
                        _testQueue1.SetField(Fields.queue_.name, "TESTQUEUE1");
                        _testQueue1.SetField(Fields.queue_.emailaddress, "testqueue1@example.com");
                        _testQueue1 = CreateAndRetrieve(_testQueue1);
                    }
                }
                return _testQueue1;
            }
        }

        private Entity _testAccount;

        public Entity TestAccount
        {
            get
            {
                if (_testAccount == null)
                {
                    _testAccount = XrmService.GetFirst("account", "name", "TESTSCRIPTACCOUNT");
                    if (_testAccount == null)
                    {
                        _testAccount = CreateAccount();
                        _testAccount.SetField("name", "TESTSCRIPTACCOUNT");
                        _testAccount = UpdateFieldsAndRetreive(_testAccount, "name");
                    }
                }
                return _testAccount;
            }
        }

        private Entity _testAccountContact;
        public Entity TestAccountContact
        {
            get
            {
                if (_testAccountContact == null)
                {
                    var contactId = TestAccount.GetLookupGuid("primarycontactid");
                    if (!contactId.HasValue)
                        throw new NullReferenceException("Test Account Doesn't Have Primary Contact");
                    _testAccountContact = XrmService.Retrieve("contact", contactId.Value);
                }
                return _testAccountContact;
            }
        }

        public void DeleteMultiple(IEnumerable<Entity> entities)
        {
            var responses = XrmService.DeleteMultiple(entities);
            if (responses.Any(r => r.Fault != null))
                Assert.Fail(responses.First(r => r.Fault != null).Fault.Message);
        }

        public void DeleteAllMatchingName(string type, IEnumerable<string> names)
        {
            var blah = XrmService.RetrieveAllOrClauses(type, names.Select(n => new ConditionExpression(XrmService.GetPrimaryNameField(type), ConditionOperator.Equal, n)));
            DeleteMultiple(blah);
        }

        public virtual IOrganizationConnectionFactory ServiceFactory
        {
            get
            {
                return new XrmOrganizationConnectionFactory();
            }
        }

    }
}