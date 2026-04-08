using JosephM.Core.Service;
using JosephM.Record.Query;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.SavedViewsExport
{
    public class SavedViewsExportService :
        ServiceBase<SavedViewsExportRequest, SavedViewsExportResponse, SavedViewsExportResponseItem>
    {
        public SavedViewsExportService(XrmRecordService service)
        {
            Service = service;
        }

        private XrmRecordService Service { get; set; }

        public override void ExecuteExtention(SavedViewsExportRequest request,
            SavedViewsExportResponse response,
            ServiceRequestController controller)
        {
            controller.LogLiteral("Loading Users");

            var savedViews = new List<SavedView>();
            var userConditions = new List<Condition>();
            if(request.IncludeAllOwningUsers)
            {
                userConditions.Add(new Condition(Fields.systemuser_.isdisabled, ConditionType.NotEqual, true));
                userConditions.Add(new Condition(Fields.systemuser_.accessmode, ConditionType.NotEqual, OptionSets.User.AccessMode.Administrative));
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
                    var savedViewConditions = new List<Condition>()
                    {
                        new Condition(Fields.userquery_.querytype, ConditionType.Equal, 0),
                        new Condition(Fields.userquery_.ownerid, ConditionType.Equal, user.Id)
                    };
                    if(!request.IncludeViewsForAllTables)
                    {
                        savedViewConditions.Add(new Condition(Fields.userquery_.returnedtypecode, ConditionType.Equal, request.SavedViewTable.Key));
                    }
                    var thisUsersSavedViews = cloneService.RetrieveAllAndClauses(Entities.userquery, savedViewConditions);
                    foreach(var savedView in thisUsersSavedViews.OrderBy(v => v.GetStringField(Fields.userquery_.name)))
                    {
                        var createdOn = savedView.GetDateTime(Fields.userquery_.createdon);
                        var modifiedOn = savedView.GetDateTime(Fields.userquery_.modifiedon);
                        savedViews.Add(new SavedView()
                        {
                            Owner = usersName,
                            RecordType = savedView.GetStringField(Fields.userquery_.returnedtypecode),
                            Name = savedView.GetStringField(Fields.userquery_.name),
                            FetchXml = savedView.GetStringField(Fields.userquery_.fetchxml),
                            LayoutXml = savedView.GetStringField(Fields.userquery_.layoutxml),
                            CreatedOn = createdOn.HasValue ? Service.LocalisationService.ConvertUtcToLocalTime(createdOn.Value) : (DateTime?) null,
                            ModifiedOn = modifiedOn.HasValue ? Service.LocalisationService.ConvertUtcToLocalTime(modifiedOn.Value) : (DateTime?)null,
                        });

                    }
                }
                catch(Exception ex)
                {
                    response.AddResponseItem(new SavedViewsExportResponseItem(new Guid(user.Id), user.GetStringField(Fields.systemuser_.fullname) ?? user.GetStringField(Fields.systemuser_.domainname), ex));
                }
            }
            response.SavedViewsExport = savedViews;
        }
    }
}