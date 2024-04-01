using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using $ext_safeprojectname$.Plugins.Core;
using $ext_safeprojectname$.Plugins.Localisation;
using $ext_safeprojectname$.Plugins.Xrm;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Schema;

namespace $safeprojectname$
{
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

        private LocalisationService _localisationService;
        public LocalisationService LocalisationService
        {
            get
            {
                if (_localisationService == null)
                {
                    _localisationService = new LocalisationService(new UserLocalisationSettings(XrmService, CurrentUserId));
                }
                return _localisationService;
            }
        }

        /// <summary>
        /// Admin Connection For Operations In The Test Script
        /// </summary>
        public XrmService XrmServiceAdmin { get; private set; }

        public IXrmConfiguration XrmConfigurationAdmin
        {
            get
            {
                return new VsixActiveXrmConnection();
            }
        }

        public Entity CreateContact(Entity account = null)
        {
            var uniqueString = DateTime.UtcNow.ToFileTime().ToString();
            return CreateTestRecord(Entities.contact, new Dictionary<string, object>
                {
                    { Fields.contact_.firstname, "Test Script" },
                    { Fields.contact_.lastname, uniqueString },
                    { Fields.contact_.emailaddress1, $"{uniqueString}@example.com" },
                    { Fields.contact_.address1_line1, "1 Test St" },
                    { Fields.contact_.address1_city, "Melbourne" },
                    { Fields.contact_.address1_stateorprovince, "VIC" },
                    { Fields.contact_.address1_postalcode, "3000" },
                    { Fields.contact_.accountid, account == null  ? null : account.ToEntityReference() }
                });
        }

        protected Entity CreateAccount()
        {
            var uniqueString = DateTime.UtcNow.ToFileTime().ToString();
            var contact = CreateContact();
            var account = CreateTestRecord(Entities.account, new Dictionary<string, object>
                    {
                        { Fields.account_.name, $"Test Account - {uniqueString}".Left(XrmServiceAdmin.GetMaxLength(Fields.account_.name, Entities.account)) },
                        { Fields.account_.emailaddress1, $"{uniqueString}@example.com" },
                        { Fields.account_.address1_line1, "1 Test St" },
                        { Fields.account_.address1_city, "Melbourne" },
                        { Fields.account_.address1_stateorprovince, "VIC" },
                        { Fields.account_.address1_postalcode, "3000" },
                        { Fields.account_.primarycontactid,contact.ToEntityReference() },
                    });
            UpdateFieldsAndRetreive(contact, new Dictionary<string, object>
                {
                    { Fields.contact_.parentcustomerid, account.ToEntityReference() }
                });
            return account;
        }

        public Entity Refresh(Entity entity)
        {
            return XrmService.Retrieve(entity.LogicalName, entity.Id);
        }

        public void Delete(Entity entity)
        {
            XrmServiceAdmin.Delete(entity);
        }

        private Guid? _currentUserId;
        public Guid CurrentUserId
        {
            get
            {
                if (!_currentUserId.HasValue)
                {
                    _currentUserId = XrmService.WhoAmI();
                }
                return _currentUserId.Value;
            }
        }

        private Guid? _currentUserIdAdmin;

        public Guid CurrentUserIdAdmin
        {
            get
            {
                if (!_currentUserIdAdmin.HasValue)
                {
                    _currentUserIdAdmin = XrmServiceAdmin.WhoAmI();
                }
                return _currentUserIdAdmin.Value;
            }
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
                        catch (Exception)
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
                        {
                            Assert.Fail("Error creating TEST SCRIPT CONTACT");
                        }
                        _testContact = XrmService.Retrieve(Entities.contact, contactId.Value);
                        _testContact = UpdateFieldsAndRetreive(_testContact, new Dictionary<string, object>
                        {
                            { Fields.contact_.firstname, "TEST SCRIPT" },
                            { Fields.contact_.lastname, "CONTACT" },
                        });
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
                        _testContact2 = UpdateFieldsAndRetreive(_testContact, new Dictionary<string, object>
                        {
                            { Fields.contact_.firstname, "TEST SCRIPT" },
                            { Fields.contact_.lastname, "CONTACT 2" },
                        });
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
                    {
                        Assert.Fail("Error getting TEST SCRIPT CONTACT account");
                    }
                    _testContactAccount = XrmService.Retrieve(Entities.account, accountId.Value);
                }
                return _testContactAccount;
            }
            set
            {
                _testContactAccount = value;
            }
        }

        public Entity CreateTestRecord(string entityType, Dictionary<string, object> fields = null, XrmService xrmService = null)
        {
            xrmService = xrmService ?? XrmService;
            var entity = new Entity(entityType);
            if (fields != null)
            {
                foreach (var field in fields)
                {
                    entity.SetField(field.Key, field.Value);
                }
            }
            entity.Id = xrmService.Create(entity);
            return xrmService.Retrieve(entity.LogicalName, entity.Id);
        }

        public Entity UpdateFieldsAndRetreive(Entity entity, Dictionary<string, object> fieldsToUpdate, XrmService xrmService = null)
        {
            xrmService = xrmService ?? XrmService;
            foreach (var fieldToUpdate in fieldsToUpdate)
            {
                entity.SetField(fieldToUpdate.Key, fieldToUpdate.Value);
            }
            xrmService.Update(entity, fieldsToUpdate.Keys.ToArray());
            return xrmService.Retrieve(entity.LogicalName, entity.Id);
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
            if (CurrentUserId == CurrentUserIdAdmin)
                return;

            //get the non-default teams th4e test user a member of
            var teamQuery = XrmServiceAdmin.BuildQuery(Entities.team,
                fields: new string[0],
                conditions: new[] { new ConditionExpression(Fields.team_.isdefault, ConditionOperator.NotEqual, true) });
            var memberJoin = teamQuery.AddLink(Relationships.team_.teammembership_association.EntityName, Fields.team_.teamid, Fields.team_.teamid);
            memberJoin.LinkCriteria.AddCondition(new ConditionExpression(Fields.systemuser_.systemuserid, ConditionOperator.Equal, CurrentUserId));
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
                    removeRequest.MemberIds = new[] { CurrentUserId };
                    XrmService.Execute(removeRequest);
                }
            }

            //add them to the team
            var addRequest = new AddMembersTeamRequest();
            addRequest.TeamId = teamId;
            addRequest.MemberIds = new[] { CurrentUserId };
            XrmService.Execute(addRequest);
        }
    }
}