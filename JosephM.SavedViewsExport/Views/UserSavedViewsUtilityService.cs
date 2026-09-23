using JosephM.Core.Service;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Views
{
    public class UserSavedViewsUtilityService :
        ServiceBase<UserSavedViewsUtilityRequest, UserSavedViewsUtilityResponse, UserSavedViewsUtilityResponseItem>
    {
        public UserSavedViewsUtilityService(XrmRecordService service)
        {
            Service = service;
        }

        private XrmRecordService Service { get; set; }

        public override void ExecuteExtention(UserSavedViewsUtilityRequest request,
            UserSavedViewsUtilityResponse response,
            ServiceRequestController controller)
        {
            controller.LogLiteral("Loading users");

            var savedObjects = new List<SavedView>();
            var userConditions = new List<Condition>();
            if(request.IncludeAllOwningUsers)
            {
                userConditions.Add(new Condition(Fields.systemuser_.isdisabled, ConditionType.NotEqual, true));
                userConditions.Add(new Condition(Fields.systemuser_.accessmode, ConditionType.NotEqual, OptionSets.User.AccessMode.Administrative));
                userConditions.Add(new Condition(Fields.systemuser_.fullname, ConditionType.NotEqual, "Provision Dynamics Provision"));
            }
            else
            {
                userConditions.Add(new Condition(Fields.systemuser_.systemuserid, ConditionType.Equal, request.Owner.Id));
            }

            var allUsers = Service.RetrieveAllAndClauses(Entities.systemuser, userConditions);

            var done = 0;
            var todo = allUsers.Count();
            var cloneService = Service.CloneForParellelProcessing() as XrmRecordService;
            foreach (var user in allUsers.OrderBy(u => u.GetStringField(Fields.systemuser_.fullname) ?? u.GetStringField(Fields.systemuser_.domainname)))
            {
                controller.UpdateProgress(done, todo, $"Processing user saved views {done}/{todo}");
                done++;
                var usersName = user.GetStringField(Fields.systemuser_.fullname) ?? user.GetStringField(Fields.systemuser_.domainname);
                try
                {
                    cloneService.ImpersonatingUserId = new Guid(user.Id);
                    var savedObjectConditions = new List<Condition>()
                    {
                        new Condition(Fields.userquery_.querytype, ConditionType.Equal, 0),
                        new Condition(Fields.userquery_.ownerid, ConditionType.Equal, user.Id)
                    };
                    if(!request.IncludeViewsForAllTables)
                    {
                        savedObjectConditions.Add(new Condition(Fields.userquery_.returnedtypecode, ConditionType.Equal, request.Table.Key));
                    }
                    var thisUsersSavedObjects = cloneService.RetrieveAllAndClauses(Entities.userquery, savedObjectConditions);
                    foreach(var savedObject in thisUsersSavedObjects.OrderBy(v => v.GetStringField(Fields.userquery_.name)))
                    {
                        var createdOn = savedObject.GetDateTime(Fields.userquery_.createdon);
                        var modifiedOn = savedObject.GetDateTime(Fields.userquery_.modifiedon);
                        savedObjects.Add(new SavedView(Service)
                        {
                            Id = savedObject.Id,
                            OwningUserId = savedObject.GetLookupId(Fields.userquery_.ownerid),
                            Owner = usersName,
                            RecordType = savedObject.GetStringField(Fields.userquery_.returnedtypecode),
                            Name = savedObject.GetStringField(Fields.userquery_.name),
                            FetchXml = savedObject.GetStringField(Fields.userquery_.fetchxml),
                            LayoutXml = savedObject.GetStringField(Fields.userquery_.layoutxml),
                            CreatedOn = createdOn.HasValue ? Service.LocalisationService.ConvertUtcToLocalTime(createdOn.Value) : (DateTime?) null,
                            ModifiedOn = modifiedOn.HasValue ? Service.LocalisationService.ConvertUtcToLocalTime(modifiedOn.Value) : (DateTime?)null,
                            Description = savedObject.GetStringField(Fields.userquery_.description)
                        });

                    }
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new UserSavedViewsUtilityResponseItem(new Guid(user.Id), user.GetStringField(Fields.systemuser_.fullname) ?? user.GetStringField(Fields.systemuser_.domainname), ex));
                }
            }
            response.SavedViews = savedObjects;
        }
    }
}