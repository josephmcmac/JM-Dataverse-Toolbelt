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
using Schema;

namespace $safeprojectname$
{
    [DeploymentItem("solution.xrmconnection")]
    [TestClass]
    public abstract class XrmTest
    {
        protected virtual IEnumerable<string> EntitiesToDelete
        {
            get { return new string[] { }; }
        }

        protected XrmTest()
        {
            Controller = new LogController();
            Controller.AddUi(new DebugUserInterface());
            XrmServiceAdmin = new XrmService(XrmConfigurationAdmin, Controller);
        }

        public LogController Controller { get; private set; }

        /// <summary>
        /// Standard User Connection For Operations In The Test Script
        /// If you need to script security roles then override this with a connection for a user with that security role
        /// </summary>
        public virtual XrmService XrmService
        {
            get
            {
                return XrmServiceAdmin;
            }
        }

        /// <summary>
        /// Admin Connection For Operations In The Test Script
        /// </summary>
        public XrmService XrmServiceAdmin { get; private set; }

        public virtual IXrmConfiguration XrmConfigurationAdmin
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
                    if (value != null && prop.Name == nameof(XrmConfigurationAdmin.Password))
                        xrmConfig.SetPropertyByString(prop.Name, new Password(value).GetRawPassword());
                    else
                        xrmConfig.SetPropertyByString(prop.Name, value);
                }
                return xrmConfig;
            }
        }

        public Entity CreateContact(Entity account)
        {
            var entity = new Entity(Entities.contact);
            entity.SetField(Fields.contact_.firstname, "Test Script");
            entity.SetField(Fields.contact_.lastname, DateTime.Now.ToFileTime().ToString());
            entity.SetField(Fields.contact_.fax, "0999999999fax");
            entity.SetField(Fields.contact_.emailaddress1, "testemail@example.com");
            entity.SetField(Fields.contact_.address1_line1, "100 Collins St");
            entity.SetField(Fields.contact_.address1_city, "Melbourne");
            entity.SetField(Fields.contact_.address1_stateorprovince, "VIC");
            entity.SetField(Fields.contact_.address1_postalcode, "3000");
            if (account != null)
                entity.SetLookupField(Fields.contact_.parentcustomerid, account);
            return CreateAndRetrieve(entity);
        }

        public virtual Entity CreateAndRetrieve(Entity entity, XrmService xrmService = null)
        {
            if (xrmService == null)
                xrmService = XrmService;
            var primaryField = xrmService.GetPrimaryField(entity.LogicalName);
            if (!entity.Contains(primaryField))
                entity.SetField(primaryField, ("Test Scripted Record" + DateTime.UtcNow.ToFileTime()).Left(xrmService.GetMaxLength(primaryField, entity.LogicalName)));
            if (entity.LogicalName == Entities.contact && !entity.Contains(Fields.contact_.firstname))
                entity.SetField(Fields.contact_.firstname, "Test");
            if (entity.LogicalName == Entities.lead && !entity.Contains(Fields.lead_.firstname))
                entity.SetField(Fields.lead_.firstname, "Test");
            var id = xrmService.Create(entity);
            return xrmService.Retrieve(entity.LogicalName, id);
        }

        public Entity Refresh(Entity entity)
        {
            return XrmService.Retrieve(entity.LogicalName, entity.Id);
        }

        public void Delete(Entity entity)
        {
            XrmServiceAdmin.Delete(entity);
        }

        public Guid CurrentUserId
        {
            get { return XrmService.WhoAmI(); }
        }

        public Guid CurrentUserIdAdmin
        {
            get { return XrmServiceAdmin.WhoAmI(); }
        }

        public void DeleteMyToday()
        {
            foreach (var entityType in EntitiesToDelete)
            {
                var userIds = new List<Guid>() { CurrentUserId };
                if (CurrentUserId != CurrentUserIdAdmin)
                    userIds.Add(CurrentUserIdAdmin);
                foreach (var userId in userIds)
                {
                    var query = XrmService.BuildQuery(entityType, new string[] { },
                        new[]
                        {
                            new ConditionExpression("createdby", ConditionOperator.Equal, userId),
                            new ConditionExpression("createdon", ConditionOperator.Today)
                        }, null);
                    var entities = XrmServiceAdmin.RetrieveAll(query);
                    foreach (var entity in entities)
                    {
                        try
                        {
                            XrmServiceAdmin.Delete(entity);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
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
                    _testContact = XrmService.GetFirst(Entities.contact, Fields.contact_.fullname, "TEST SCRIPT CONTACT");
                    if (_testContact == null)
                    {
                        var account = CreateAccount();
                        var contactId = account.GetLookupGuid(Fields.account_.primarycontactid);
                        if (!contactId.HasValue)
                            throw new NullReferenceException();
                        _testContact = XrmService.Retrieve(Entities.contact, contactId.Value);
                        _testContact.SetField(Fields.contact_.firstname, "TEST SCRIPT");
                        _testContact.SetField(Fields.contact_.lastname, "CONTACT");
                        _testContact.SetField(Fields.contact_.salutation, "Mr");
                        _testContact.SetField(Fields.contact_.nickname, "iTest");
                        _testContact.SetField(Fields.contact_.birthdate, new DateTime(1080, 11, 15));
                        _testContact.SetField(Fields.contact_.gendercode, new OptionSetValue(1));
                        _testContact = UpdateFieldsAndRetreive(_testContact, Fields.contact_.firstname, Fields.contact_.lastname);
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
                    _testContact2 = XrmService.GetFirst(Entities.contact, Fields.contact_.fullname, "TEST SCRIPT CONTACT 2");
                    if (_testContact2 == null)
                    {
                        _testContact2 = CreateContact(TestContactAccount);
                        _testContact2.SetField(Fields.contact_.firstname, "TEST SCRIPT");
                        _testContact2.SetField(Fields.contact_.lastname, "CONTACT 2");
                        _testContact2 = UpdateFieldsAndRetreive(_testContact2, Fields.contact_.firstname, Fields.contact_.lastname);
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
                    var accountId = TestContact.GetLookupGuid(Fields.contact_.parentcustomerid);
                    if (!accountId.HasValue)
                        throw new NullReferenceException();
                    _testContactAccount = XrmService.Retrieve(Entities.account, accountId.Value);
                }
                return _testContactAccount;
            }
            set
            {
                _testContactAccount = value;
            }
        }

        public virtual Entity UpdateFieldsAndRetreive(Entity entity, params string[] fieldsToUpdate)
        {
            return XrmService.UpdateAndRetrieve(entity, fieldsToUpdate);
        }

        protected Entity CreateAccount()
        {
            var entity = new Entity(Entities.account);
            var maxNameLength = XrmService.GetMaxLength(Fields.account_.name, Entities.account);
            entity.SetField(Fields.account_.name, "Test Account - " + DateTime.Now.ToLocalTime());
            entity.SetField(Fields.account_.fax, "0999999999fax");
            entity.SetField(Fields.account_.telephone1, "0999999999");
            entity.SetField(Fields.account_.emailaddress1, "testfakeemail@example.com");
            entity.SetField(Fields.account_.address1_line1, "100 Collins St");
            entity.SetField(Fields.account_.address1_city, "Melbourne");
            entity.SetField(Fields.account_.address1_stateorprovince, "VIC");
            entity.SetField(Fields.account_.address1_postalcode, "3000");
            Entity contact = CreateContact(null);
            entity.SetLookupField(Fields.account_.primarycontactid, contact);

            var account = CreateAndRetrieve(entity);
            contact.SetLookupField(Fields.contact_.parentcustomerid, account);
            XrmService.Update(contact, new[] { Fields.contact_.parentcustomerid });
            return account;
        }

        public Entity CreateTestRecord(string entityType, Dictionary<string, object> fields = null, XrmService xrmService = null)
        {
            var entity = new Entity(entityType);
            if (fields != null)
            {
                foreach (var field in fields)
                {
                    entity.SetField(field.Key, field.Value);
                }
            }
            return CreateAndRetrieve(entity, xrmService);
        }

        public T CreateWorkflowInstance<T>(Entity target = null)
             where T : XrmWorkflowActivityInstanceBase, new()
        {
            var instance = new T();
            instance.XrmService = XrmService;
            instance.LogController = Controller;
            instance.InitiatingUserId = XrmService.WhoAmI();
            instance.IsSandboxIsolated = true;
            if (target != null)
            {
                instance.TargetId = target.Id;
                instance.TargetType = target.LogicalName;
            }
            return instance;
        }

        /// <summary>
        /// If there is a separate user connection for the XrmService connection used for scripting limited security
        /// sets that connections user as a member of the teamId and removes them from any other team
        /// </summary>
        /// <param name="teamId">id of the team to set the test script user in</param>
        public void SetTestUserAsTeamMember(Guid teamId)
        {

            var testUserId = XrmService.WhoAmI();
            var adminUserId = XrmServiceAdmin.WhoAmI();
            //if the same user for both the admin and standard connection then don't bother 
            if (testUserId == adminUserId)
                return;

            //get the non-default teams th4e test user a member of
            var teamQuery = XrmServiceAdmin.BuildQuery(Entities.team,
                fields: new string[0],
                conditions: new[] { new ConditionExpression(Fields.team_.isdefault, ConditionOperator.NotEqual, true) });
            var memberJoin = teamQuery.AddLink(Relationships.team_.teammembership_association.EntityName, Fields.team_.teamid, Fields.team_.teamid);
            memberJoin.LinkCriteria.AddCondition(new ConditionExpression(Fields.systemuser_.systemuserid, ConditionOperator.Equal, testUserId));
            var teamsMemberships = XrmServiceAdmin.RetrieveAll(teamQuery);

            //if they are already only a member of the team then return
            if (teamsMemberships.Count() == 1 && teamsMemberships.First().Id == teamId)
                return;

            //remove them from the teams they are a member of
            foreach (var team in teamsMemberships)
            {
                if (!team.GetBoolean(Fields.team_.isdefault))
                {
                    var removeRequest = new RemoveMembersTeamRequest();
                    removeRequest.TeamId = team.Id;
                    removeRequest.MemberIds = new[] { testUserId };
                    XrmService.Execute(removeRequest);
                }
            }

            //add them to the team
            var addRequest = new AddMembersTeamRequest();
            addRequest.TeamId = teamId;
            addRequest.MemberIds = new[] { testUserId };
            XrmService.Execute(addRequest);
        }
    }
}