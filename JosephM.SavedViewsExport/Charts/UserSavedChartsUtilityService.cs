using JosephM.Core.Service;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Charts
{
    public class UserSavedChartsUtilityService :
        ServiceBase<UserSavedChartsUtilityRequest, UserSavedChartsUtilityResponse, UserSavedChartsUtilityResponseItem>
    {
        public UserSavedChartsUtilityService(XrmRecordService service)
        {
            Service = service;
        }

        private XrmRecordService Service { get; set; }

        public override void ExecuteExtention(UserSavedChartsUtilityRequest request,
            UserSavedChartsUtilityResponse response,
            ServiceRequestController controller)
        {
            controller.LogLiteral("Loading users");

            var savedObjects = new List<SavedChart>();
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
                controller.UpdateProgress(done, todo, $"Processing user saved Charts {done}/{todo}");
                done++;
                var usersName = user.GetStringField(Fields.systemuser_.fullname) ?? user.GetStringField(Fields.systemuser_.domainname);
                try
                {
                    cloneService.ImpersonatingUserId = new Guid(user.Id);
                    var savedObjectConditions = new List<Condition>()
                    {
                        new Condition(Fields.userqueryvisualization_.ownerid, ConditionType.Equal, user.Id)
                    };
                    if (!request.IncludeViewsForAllTables)
                    {
                        savedObjectConditions.Add(new Condition(Fields.userqueryvisualization_.primaryentitytypecode, ConditionType.Equal, request.Table.Key));
                    }
                    var thisUsersSavedCharts = cloneService.RetrieveAllAndClauses(Entities.userqueryvisualization, savedObjectConditions);
                    foreach(var savedView in thisUsersSavedCharts.OrderBy(v => v.GetStringField(Fields.userqueryvisualization_.name)))
                    {
                        var createdOn = savedView.GetDateTime(Fields.userqueryvisualization_.createdon);
                        var modifiedOn = savedView.GetDateTime(Fields.userqueryvisualization_.modifiedon);
                        savedObjects.Add(new SavedChart(Service)
                        {
                            Id = savedView.Id,
                            OwningUserId = savedView.GetLookupId(Fields.userqueryvisualization_.ownerid),
                            Owner = usersName,
                            RecordType = savedView.GetStringField(Fields.userqueryvisualization_.primaryentitytypecode),
                            Name = savedView.GetStringField(Fields.userqueryvisualization_.name),
                            CreatedOn = createdOn.HasValue ? Service.LocalisationService.ConvertUtcToLocalTime(createdOn.Value) : (DateTime?) null,
                            ModifiedOn = modifiedOn.HasValue ? Service.LocalisationService.ConvertUtcToLocalTime(modifiedOn.Value) : (DateTime?)null,
                            ChartPresentationXml = savedView.GetStringField(Fields.userqueryvisualization_.presentationdescription),
                            ChartDataXml = savedView.GetStringField(Fields.userqueryvisualization_.datadescription),
                            Description = savedView.GetStringField(Fields.userqueryvisualization_.description)
                        });

                    }
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new UserSavedChartsUtilityResponseItem(new Guid(user.Id), user.GetStringField(Fields.systemuser_.fullname) ?? user.GetStringField(Fields.systemuser_.domainname), ex));
                }
            }
            response.SavedCharts = savedObjects;
        }
    }
}