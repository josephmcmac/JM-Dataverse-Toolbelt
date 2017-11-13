#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using JosephM.Core.Log;
using JosephM.Core.Test;
using Microsoft.Xrm.Sdk.Query;

#endregion

namespace JosephM.Xrm.Test
{
    public abstract class XrmTest : CoreTest
    {
        public static string TestXrmConnectionFileName
        {
            get { return Path.Combine(TestConstants.TestSettingsFolder, "TestXrmConnection.xml"); }
        }

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
                    _xrmService = new XrmService(XrmConfiguration, Controller);
                return _xrmService;
            }
        }

        protected IUserInterface UI { get; private set; }

        public virtual IXrmConfiguration XrmConfiguration
        {
            get
            {
                var settingsObject = GetSavedTestEncryptedXrmConfiguration();
                return new XrmConfiguration()
                {
                    AuthenticationProviderType = settingsObject.AuthenticationProviderType,
                    DiscoveryServiceAddress = settingsObject.DiscoveryServiceAddress,
                    OrganizationUniqueName = OverrideOrganisation ?? settingsObject.OrganizationUniqueName,
                    Domain = settingsObject.Domain,
                    Username = settingsObject.Username,
                    Password = settingsObject.Password == null ? null : settingsObject.Password.GetRawPassword()
                };
            }
        }

        protected static EncryptedXrmConfiguration GetSavedTestEncryptedXrmConfiguration()
        {
            EncryptedXrmConfiguration settingsObject = null;
            var serializer = new DataContractSerializer(typeof (EncryptedXrmConfiguration));
            if (!File.Exists(TestXrmConnectionFileName))
                throw new NullReferenceException(
                    string.Format(
                        "Error The Xrm Test Settings File Was Not Found At {0}. Create It Using The Save Xrm Connection Option In The Test Prism Application",
                        TestXrmConnectionFileName));
            using (var fileStream = new FileStream(TestXrmConnectionFileName, FileMode.Open))
            {
                settingsObject = (EncryptedXrmConfiguration) serializer.ReadObject(fileStream);
            }
            return settingsObject;
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

        public void DeleteAll(string entityType)
        {
            var query = new QueryExpression(entityType);
            var items = XrmService.RetrieveAll(query);
            XrmService.DeleteMultiple(items);
        }

        public Entity CreateTestRecord(string entityType)
        {
            var entity = new Entity(entityType);
            return CreateAndRetrieve(entity);
        }

        public Entity CreateAndRetrieve(Entity entity)
        {
            var primaryField = XrmService.GetPrimaryNameField(entity.LogicalName);
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
            return XrmService.CreateAndRetreive(entity);
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

        public Entity CreateRecordAllFieldsPopulated(string type)
        {
            var entity = new Entity(type);
            var fieldsToExlcude = new[] { "traversedpath" };
            var fields = XrmService.GetFields(type)
                .Where(f => !fieldsToExlcude.Contains(f));
            foreach (var field in fields)
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
                                entity.SetField(field, 1);
                                break;
                            }
                        case AttributeTypeCode.Lookup:
                            {
                                var target = XrmService.GetLookupTargetEntity(field, type);
                                var typesToExlcude = new[]
                            {
                                "equipment", "transactioncurrency", "pricelevel", "service", "systemuser", "incident",
                                "campaign", "territory", "sla", "bookableresource", "msdyn_taxcode"
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
                                entity.SetField(field, "1234");
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
                    var user = XrmService.RetrieveFirst(XrmService.BuildQuery("systemuser", new string[0], conditions, null));
                    if (user == null)
                        throw new NullReferenceException("Could not find other user");
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

        public Entity CreateTestRecord(string entityType, Dictionary<string, object> fields)
        {
            var entity = new Entity(entityType);
            foreach (var field in fields)
            {
                entity.SetField(field.Key, field.Value);
            }
            return CreateAndRetrieve(entity);
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
    }
}