using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using $ext_safeprojectname$.Plugins.Core;
using $ext_safeprojectname$.Plugins.Xrm;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

namespace $safeprojectname$
{
    [TestClass]
    public abstract class XrmTest
    {
        private Entity _transactionCurrency;

        public Entity TransactionCurrency
        {
            get
            {
                if (_transactionCurrency == null)
                    _transactionCurrency = XrmService.GetFirst("transactioncurrency");
                return _transactionCurrency;
            }
        }

        protected virtual IEnumerable<string> EntitiesToDelete
        {
            get { return new string[] { }; }
        }

        protected XrmTest()
        {
            Controller = new LogController();
            XrmService = new XrmService(XrmConfiguration, Controller);
        }

        protected LogController Controller { get; private set; }
        public XrmService XrmService { get; private set; }

        public virtual IXrmConfiguration XrmConfiguration
        {
            get
            {
                var readEncryptedConfig = File.ReadAllText("solution.xrmconnection");
                var dictionary =
                    (Dictionary<string, string>)
                        JsonHelper.JsonStringToObject(readEncryptedConfig, typeof(Dictionary<string, string>));

                var xrmConfig = new XrmConfiguration();
                foreach (var prop in xrmConfig.GetType().GetReadWriteProperties())
                {
                    var value = dictionary[prop.Name];
                    if (value != null && prop.Name == nameof(XrmConfiguration.Password))
                        xrmConfig.SetPropertyByString(prop.Name, new Password(value).GetRawPassword());
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


        public Entity CreateContact(Entity account)
        {
            return CreateContact(account, false);
        }

        public Entity CreateContact(Entity account, bool usePrimaryAddress)
        {
            var entity = new Entity("contact");
            entity.SetField("firstname", "Test Script");
            entity.SetField("lastname", DateTime.Now.ToFileTime().ToString());
            entity.SetField("fax", "0999999999fax");
            entity.SetField("emailaddress1", "testemail@ntaadev.com");
            if (entity.GetField("address1_line1") == null)
            {
                entity.SetField("address1_line1", "9/455 Bourke St");
                entity.SetField("address1_city", "Melbourne");
                entity.SetField("address1_stateorprovince", "VIC");
                entity.SetField("address1_postalcode", "3000");
            }
            if (account != null)
                entity.SetLookupField("parentcustomerid", account);
            return CreateAndRetrieve(entity);
        }

        public void LogLiteral(string message)
        {
            Controller.LogLiteral(message);
        }

        public void PrepareForTest(bool isPluginDebuggerOn)
        {
        }

        public virtual void PrepareForTest()
        {
            PrepareForTest(false);
        }

        public void CheckException(Exception ex)
        {
            if (ex is AssertFailedException)
            {
                Controller.LogLiteral("ASSERT FAILED");
                throw ex;
            }
        }

        public void VerifyCreateOrUpdateError(Entity workflow)
        {
            try
            {
                if (workflow.Id == Guid.Empty)
                    XrmService.Create(workflow);
                else
                    XrmService.Update(workflow);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsFalse(ex is AssertFailedException);
            }
        }

        public void DeleteAllData()
        {
            foreach (var entityType in EntitiesToDelete)
            {
                DeleteAll(entityType);
            }
        }

        public void DeleteAll(string entityType)
        {
            var query = new QueryExpression(entityType);
            var items = XrmService.RetrieveAll(query);
            XrmService.DeleteMultiple(items);
        }

        public void DeleteRecentData()
        {
            foreach (var entityType in EntitiesToDelete)
            {
                var query = new QueryExpression(entityType);
                query.Criteria.AddCondition("createdon", ConditionOperator.GreaterThan, DateTime.Now.AddMinutes(-30));
                var items = XrmService.RetrieveAll(query);
                XrmService.DeleteMultiple(items);
            }
        }

        public Entity CreateTestRecord(string entityType)
        {
            var entity = new Entity(entityType);
            return CreateAndRetrieve(entity);
        }

        public virtual Entity CreateAndRetrieve(Entity entity)
        {
            var primaryField = XrmService.GetPrimaryNameField(entity.LogicalName);
            if (!entity.Contains(primaryField))
                entity.SetField(primaryField, "Test Scripted Record".Left(XrmService.GetMaxLength(primaryField, entity.LogicalName)));
            if (entity.LogicalName == "contact" && !entity.Contains("firstname"))
                entity.SetField("firstname", "Test");
            if (entity.LogicalName == "lead" && !entity.Contains("firstname"))
                entity.SetField("firstname", "Test");
            if (!entity.Contains("transactioncurrencyid") && XrmService.FieldExists("transactioncurrencyid", entity.LogicalName))
                entity.SetLookupField("transactioncurrencyid", TransactionCurrency);
            return XrmService.CreateAndRetreive(entity);
        }

        public Entity UpdateAndRetreive(Entity entity)
        {
            return UpdateAndRetreive(entity, null);
        }

        public Entity UpdateAndRetreive(Entity entity, IEnumerable<string> fieldsToGet)
        {
            XrmService.Update(entity);
            return XrmService.Retrieve(entity.LogicalName, entity.Id, fieldsToGet);
        }

        public Entity UpdateFieldsAndRetreive(Entity entity, IEnumerable<string> fieldsToUpdate)
        {
            XrmService.Update(entity, fieldsToUpdate);
            return XrmService.Retrieve(entity.LogicalName, entity.Id);
        }

        public Entity Refresh(Entity entity)
        {
            return XrmService.Refresh(entity);
        }

        public string TestEntityType
        {
            get { return "new_testentity"; }
        }

        public void Delete(Entity entity)
        {
            XrmService.Delete(entity);
        }

        public Guid CurrentUserId
        {
            get { return XrmService.WhoAmI(); }
        }

        public void DeleteMyToday()
        {
            foreach (var entity in EntitiesToDelete)
                DeleteMyToday(entity);
        }

        public void DeleteMyToday(string entityType)
        {
            var query = XrmService.BuildQuery(entityType, new string[] { },
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

        protected Entity CreateInvoice(Entity customer, decimal amount)
        {
            var invoice = new Entity("invoice");
            invoice.SetField("name", "Test Invoice");
            invoice.SetLookupField("customerid", customer);
            invoice.SetLookupField("pricelevelid", PriceList);
            invoice = CreateAndRetrieve(invoice);
            var invoiceDetail = new Entity("invoicedetail");
            invoiceDetail.SetField("isproductoverridden", true);
            invoiceDetail.SetField("ispriceoverridden", true);
            invoiceDetail.SetField("quantity", (decimal)1);
            invoiceDetail.SetMoneyField("priceperunit", amount);
            invoiceDetail.SetField("productdescription", "Test Invoice Detail");
            invoiceDetail.SetLookupField("invoiceid", invoice);
            CreateAndRetrieve(invoiceDetail);
            return Refresh(invoice);
        }

        public Entity CreateInvoice(Entity account, int amount, Entity order)
        {
            var invoice = new Entity("invoice");
            invoice.SetField("name", "Test Invoice");
            invoice.SetLookupField("customerid", account);
            invoice.SetLookupField("salesorderid", order);
            invoice.SetLookupField("pricelevelid", PriceList);
            invoice = CreateAndRetrieve(invoice);
            var invoiceDetail = new Entity("invoicedetail");
            invoiceDetail.SetField("isproductoverridden", true);
            invoiceDetail.SetField("ispriceoverridden", true);
            invoiceDetail.SetField("quantity", (decimal)1);
            invoiceDetail.SetMoneyField("priceperunit", amount);
            invoiceDetail.SetField("productdescription", "Test Invoice Detail");
            invoiceDetail.SetLookupField("invoiceid", invoice);
            CreateAndRetrieve(invoiceDetail);
            return Refresh(invoice);
        }

        protected Entity CreateInvoice(Guid accountId, decimal amount)
        {
            var invoice = new Entity("invoice");
            invoice.SetField("name", "Test Invoice");
            invoice.SetLookupField("customerid", accountId, "account");
            invoice.SetLookupField("pricelevelid", PriceList);
            invoice = CreateAndRetrieve(invoice);
            var invoiceDetail = new Entity("invoicedetail");
            invoiceDetail.SetField("isproductoverridden", true);
            invoiceDetail.SetField("ispriceoverridden", true);
            invoiceDetail.SetField("quantity", (decimal)1);
            invoiceDetail.SetMoneyField("priceperunit", amount);
            invoiceDetail.SetField("productdescription", "Test Invoice Detail");
            invoiceDetail.SetLookupField("invoiceid", invoice);
            CreateAndRetrieve(invoiceDetail);
            return Refresh(invoice);
        }

        private Entity _priceList;

        protected Entity PriceList
        {
            get
            {
                if (_priceList == null)
                {
                    _priceList = XrmService.GetFirst("pricelevel");
                    if (_priceList == null)
                    {
                        var entity = new Entity("pricelevel");
                        entity.SetField("name", "Test Price List");
                        _priceList = CreateAndRetrieve(entity);
                    }
                }
                return _priceList;
            }
        }

        private Entity _uom;

        protected Entity Uom
        {
            get
            {
                if (_uom == null)
                {
                    _uom = XrmService.GetFirst("uom");
                }
                return _uom;
            }
        }


        private Entity _testProduct;

        public Entity TestProduct
        {
            get
            {
                if (_testProduct == null)
                {
                    _testProduct = XrmService.GetFirst("product", "name", "TESTSCRIPTPRODUCT");
                    if (_testProduct == null)
                    {
                        _testProduct = new Entity("product");
                        _testProduct.SetField("name", "TESTSCRIPTPRODUCT");
                        _testProduct.SetField("productnumber", "TESTSCRIPTPRODUCT");
                        SetRequiredProductFields(_testProduct);
                        _testProduct = CreateAndRetrieve(_testProduct);
                    }
                }
                return _testProduct;
            }
            set { _testProduct = value; }
        }

        public void SetRequiredProductFields(Entity product)
        {
            if (product.GetStringField("productnumber").IsNullOrWhiteSpace())
                product.SetField("productnumber", "TEST" + DateTime.Now.ToFileTime());
            product.SetLookupField("defaultuomid", Uom);
            product.SetField("defaultuomscheduleid", Uom.GetField("uomscheduleid"));
            product.SetLookupField("transactioncurrencyid", XrmService.GetFirst("transactioncurrency"));
        }

        public virtual string SqlServer
        {
            get { return null; }
        }

        public virtual string SqlDatabase
        {
            get { return null; }
        }

        public void UpdateSql(Entity target, IEnumerable<string> fieldsToUpdate)
        {
            UpdateSql(XrmService.ReplicateWithFields(target, fieldsToUpdate));
        }

        private string WrapSqlString(string astring)
        {
            return string.Format("'{0}'", astring);
        }

        public void UpdateSql(Entity target)
        {
            var type = target.LogicalName;
            var table = type; // + "ExtensionBase";

            var fieldsToUpdate = new List<KeyValuePair<string, string>>();
            foreach (var field in target.GetFieldsInEntity())
            {
                var fieldType = XrmService.GetFieldType(field, type);
                var value = target.GetField(field);
                if (value == null)
                {
                    if (fieldType == AttributeTypeCode.Money)
                    {
                        fieldsToUpdate.Add(new KeyValuePair<string, string>(field + "_base", "null"));
                    }
                    fieldsToUpdate.Add(new KeyValuePair<string, string>(field, "null"));
                }
                else
                {
                    switch (fieldType)
                    {
                        case AttributeTypeCode.DateTime:
                            fieldsToUpdate.Add(new KeyValuePair<string, string>(field,
                                SqlProvider.ToSqlDateString((DateTime)value)));
                            break;
                        case AttributeTypeCode.Money:
                            fieldsToUpdate.Add(new KeyValuePair<string, string>(field,
                                XrmEntity.GetMoneyValue(value).ToString(CultureInfo.InvariantCulture)));
                            fieldsToUpdate.Add(new KeyValuePair<string, string>(field + "_base",
                                XrmEntity.GetMoneyValue(value).ToString(CultureInfo.InvariantCulture)));
                            break;
                        case AttributeTypeCode.Lookup:
                            var id = XrmEntity.GetLookupGuid(value);
                            if (!id.HasValue)
                                throw new NullReferenceException("error no id in " + field);
                            fieldsToUpdate.Add(new KeyValuePair<string, string>(field,
                                WrapSqlString(id.Value.ToString())));
                            break;
                        case AttributeTypeCode.Picklist:
                            fieldsToUpdate.Add(new KeyValuePair<string, string>(field,
                                XrmEntity.GetOptionSetValue(value).ToString(CultureInfo.InvariantCulture)));
                            break;
                        case AttributeTypeCode.Status:
                            fieldsToUpdate.Add(new KeyValuePair<string, string>(field,
                                XrmEntity.GetOptionSetValue(value).ToString(CultureInfo.InvariantCulture)));
                            break;
                        case AttributeTypeCode.Integer:
                            fieldsToUpdate.Add(new KeyValuePair<string, string>(field, value.ToString()));
                            break;
                        case AttributeTypeCode.Decimal:
                            fieldsToUpdate.Add(new KeyValuePair<string, string>(field, value.ToString()));
                            break;
                        case AttributeTypeCode.Boolean:
                            fieldsToUpdate.Add(new KeyValuePair<string, string>(field,
                                XrmEntity.GetBoolean(value) ? "1" : "0"));
                            break;
                        case AttributeTypeCode.String:
                            fieldsToUpdate.Add(new KeyValuePair<string, string>(field,
                                SqlProvider.ToSqlString((string)value)));
                            break;
                        case AttributeTypeCode.Memo:
                            fieldsToUpdate.Add(new KeyValuePair<string, string>(field,
                                SqlProvider.ToSqlString((string)value)));
                            break;
                        case AttributeTypeCode.Uniqueidentifier:
                            break;
                        default:
                            throw new NotImplementedException("No update logic implemented for field type " + fieldType);
                    }
                }
            }
            var setStrings = string.Join(",",
                fieldsToUpdate.Select(kv => string.Format("{0} = {1}", kv.Key, kv.Value)));
            var primaryKey = XrmService.GetPrimaryKeyField(target.LogicalName);
            var primaryKeySql = SqlProvider.ToSqlString(target.Id);
            var sql = string.Format("update {0} set {1} where {2} = {3}", table, setStrings, primaryKey, primaryKeySql);
            SqlProvider.ExecuteNonQuery(sql);
        }


        private SqlProvider _sqlProvider;

        public SqlProvider SqlProvider
        {
            get
            {
                if (_sqlProvider == null)
                {
                    _sqlProvider = new SqlProvider(SqlServer, SqlDatabase);
                }
                return _sqlProvider;
            }
        }

        public void WaitTillTrue(Func<bool> assertInTime, int seconds)
        {
            var secondsWaited = 0;
            while (!assertInTime())
            {
                secondsWaited++;
                if (secondsWaited > seconds)
                    Assert.Fail("Waited Too Long Without Meeting Test Criteria");
                Thread.Sleep(1000);
            }
        }

        private Entity _testContact;

        public Entity TestContact
        {
            get
            {
                if (_testContact == null)
                {
                    _testContact = XrmService.GetFirst("contact", "fullname", "TEST SCRIPT CONTACT");
                    if (_testContact == null)
                    {
                        var account = CreateAccount();
                        var contactId = account.GetLookupGuid("primarycontactid");
                        if (!contactId.HasValue)
                            throw new NullReferenceException();
                        _testContact = XrmService.Retrieve("contact", contactId.Value);
                        _testContact.SetField("firstname", "TEST SCRIPT");
                        _testContact.SetField("lastname", "CONTACT");
                        _testContact = UpdateFieldsAndRetreive(_testContact, "firstname", "lastname");
                    }
                }
                return _testContact;
            }
            set { _testContact = value; }
        }

        private Entity _testContact2;

        public Entity TestContact2
        {
            get
            {
                if (_testContact2 == null)
                {
                    _testContact2 = XrmService.GetFirst("contact", "fullname", "TEST SCRIPT CONTACT 2");
                    if (_testContact2 == null)
                    {
                        _testContact2 = CreateContact(TestContactAccount);
                        _testContact2.SetField("firstname", "TEST SCRIPT");
                        _testContact2.SetField("lastname", "CONTACT 2");
                        _testContact2 = UpdateFieldsAndRetreive(_testContact2, "firstname", "lastname");
                    }
                }
                return _testContact2;
            }
            set { _testContact2 = value; }
        }

        private Entity _testContactAccount;

        public Entity TestContactAccount
        {
            get
            {
                if (_testContactAccount == null)
                {
                    var accountId = TestContact.GetLookupGuid("parentcustomerid");
                    if (!accountId.HasValue)
                        throw new NullReferenceException();
                    _testContactAccount = XrmService.Retrieve("account", accountId.Value);
                }
                return _testContactAccount;
            }
        }

        public virtual Entity UpdateFieldsAndRetreive(Entity entity, params string[] fieldsToUpdate)
        {
            XrmService.Update(entity, fieldsToUpdate);
            return XrmService.Retrieve(entity.LogicalName, entity.Id);
        }

        protected Entity CreateAccount()
        {
            var entity = new Entity("account");
            var maxNameLength = XrmService.GetMaxLength("name", "account");
            entity.SetField("name", "Test Account - " + DateTime.Now.ToLocalTime());
            entity.SetField("fax", "0999999999fax");
            entity.SetField("telephone1", "0999999999");
            entity.SetField("emailaddress1", "testemail@ntaadev.com");
            if (entity.GetField("address1_line1") == null)
            {
                entity.SetField("address1_line1", "9/455 Bourke St");
                entity.SetField("address1_city", "Melbourne");
                entity.SetField("address1_stateorprovince", "VIC");
                entity.SetField("address1_postalcode", "3000");
            }
            Entity contact = null;
            if (entity.GetField("primarycontactid") == null)
            {
                contact = CreateContact(null);
                entity.SetLookupField("primarycontactid", contact);
            }

            var account = CreateAndRetrieve(entity);
            if (contact != null)
            {
                contact.SetLookupField("parentcustomerid", account);
                UpdateFieldsAndRetreive(contact, new[] { "parentcustomerid" });
            }
            return account;
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

        public IEnumerable<Entity> GetRegardingEmails(Entity regardingObject)
        {
            return XrmService.RetrieveAllAndClauses("email",
                new[] { new ConditionExpression("regardingobjectid", ConditionOperator.Equal, regardingObject.Id) }, null);
        }

        public IEnumerable<Entity> GetRegardingTasks(Entity regardingObject)
        {
            return XrmService.RetrieveAllAndClauses("task",
                new[] { new ConditionExpression("regardingobjectid", ConditionOperator.Equal, regardingObject.Id) }, null);
        }

        public IEnumerable<Entity> GetRegardingNotes(Entity regardingObject)
        {
            return XrmService.RetrieveAllAndClauses("annotation",
                new[] { new ConditionExpression("objectid", ConditionOperator.Equal, regardingObject.Id) }, null);
        }

        public T CreateWorkflowInstance<T>()
            where T : XrmWorkflowActivityInstanceBase, new()
        {
            var instance = new T();
            instance.XrmService = XrmService;
            instance.LogController = Controller;
            return instance;
        }

        public T CreateWorkflowInstance<T>(Entity target)
    where T : XrmWorkflowActivityInstanceBase, new()
        {
            var instance = CreateWorkflowInstance<T>();
            instance.TargetId = target.Id;
            instance.TargetType = target.LogicalName;
            instance.IsSandboxIsolated = true;
            return instance;
        }

        public IEnumerable<Entity> GetConnections(Entity entity)
        {
            var firstContactConnections = XrmService.RetrieveAllAndClauses("connection",
                new[]
                {
                    new ConditionExpression("record1id", ConditionOperator.Equal,
                        entity.Id)
                });
            return firstContactConnections;
        }

        public static DateTime GetTodayUnspecifiedZone()
        {
            var now = DateTime.Now;
            //don't use datetime.today as crm converts timezone in some scnearios
            //this is unspecified zone
            var today = new DateTime(now.Year, now.Month, now.Day);
            return today;
        }

        public void AddToTeam(Guid userId, Guid teamId)
        {
            if (!IsInTeam(userId, teamId))
            {
                var request = new AddMembersTeamRequest();
                request.TeamId = teamId;
                request.MemberIds = new[] { userId };
                XrmService.Execute(request);
            }
        }

        public void RemoveFromTeam(Guid userId, Guid teamId)
        {
            if (IsInTeam(userId, teamId))
            {
                var request = new RemoveMembersTeamRequest();
                request.TeamId = teamId;
                request.MemberIds = new[] { userId };
                XrmService.Execute(request);
            }
        }

        public bool IsInTeam(Guid userId, Guid teamId)
        {
            var query = XrmService.BuildQuery("teammembership", new string[0],
                new[]
                {
                    new ConditionExpression("systemuserid", ConditionOperator.Equal, userId),
                    new ConditionExpression("teamid", ConditionOperator.Equal, teamId),
                }, null);
            return XrmService.RetrieveFirst(query) != null;
        }

        public void PopulateField(string field, Entity entity)
        {
            var type = entity.LogicalName;
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
                                "campaign", "territory"
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
}