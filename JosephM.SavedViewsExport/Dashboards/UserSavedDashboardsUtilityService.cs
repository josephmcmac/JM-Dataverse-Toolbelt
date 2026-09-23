using JosephM.Core.Service;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Dashboards
{
    public class UserSavedDashboardsUtilityService :
        ServiceBase<UserSavedDashboardsUtilityRequest, UserSavedDashboardsUtilityResponse, UserSavedDashboardsUtilityResponseItem>
    {
        public UserSavedDashboardsUtilityService(XrmRecordService service)
        {
            Service = service;
        }

        private XrmRecordService Service { get; set; }

        public override void ExecuteExtention(UserSavedDashboardsUtilityRequest request,
            UserSavedDashboardsUtilityResponse response,
            ServiceRequestController controller)
        {
            controller.LogLiteral("Loading users");

            var savedObjects = new List<SavedDashboard>();
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
                controller.UpdateProgress(done, todo, $"Processing user saved Dashboards {done}/{todo}");
                done++;
                var usersName = user.GetStringField(Fields.systemuser_.fullname) ?? user.GetStringField(Fields.systemuser_.domainname);
                try
                {
                    cloneService.ImpersonatingUserId = new Guid(user.Id);
                    var savedObjectConditions = new List<Condition>()
                    {
                        new Condition(Fields.userform_.ownerid, ConditionType.Equal, user.Id)
                    };
                    var thisUsersSavedObjects = cloneService.RetrieveAllAndClauses(Entities.userform, savedObjectConditions);
                    foreach(var savedObject in thisUsersSavedObjects.OrderBy(v => v.GetStringField(Fields.userform_.name)))
                    {
                        var createdOn = savedObject.GetDateTime(Fields.userform_.createdon);
                        var modifiedOn = savedObject.GetDateTime(Fields.userform_.modifiedon);
                        savedObjects.Add(new SavedDashboard(Service)
                        {
                            Id = savedObject.Id,
                            OwningUserId = savedObject.GetLookupId(Fields.userform_.ownerid),
                            Owner = usersName,
                            Name = savedObject.GetStringField(Fields.userform_.name),
                            CreatedOn = createdOn.HasValue ? Service.LocalisationService.ConvertUtcToLocalTime(createdOn.Value) : (DateTime?) null,
                            ModifiedOn = modifiedOn.HasValue ? Service.LocalisationService.ConvertUtcToLocalTime(modifiedOn.Value) : (DateTime?)null,
                            FormXml = savedObject.GetStringField(Fields.userform_.formxml),
                            Description = savedObject.GetStringField(Fields.userform_.description)
                        });
                    }
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new UserSavedDashboardsUtilityResponseItem(new Guid(user.Id), user.GetStringField(Fields.systemuser_.fullname) ?? user.GetStringField(Fields.systemuser_.domainname), ex));
                }
            }
            response.SavedDashboards = savedObjects;
        }
    }
}