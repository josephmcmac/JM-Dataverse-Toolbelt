using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Dashboards.Share
{
    public class ShareSavedDashboardService :
        ServiceBase<ShareSavedDashboardRequest, ShareSavedDashboardResponse, ShareSavedDashboardResponseItem>
    {
        public XrmRecordService RecordService { get; set; }
        public ShareSavedDashboardService(XrmRecordService recordService)
        {
            RecordService = recordService;
        }

        public override void ExecuteExtention(ShareSavedDashboardRequest request, ShareSavedDashboardResponse response,
            ServiceRequestController controller)
        {
            var countToUpdate = request.RecordCount;
            controller.UpdateProgress(0, countToUpdate, "Sharing Views");
            var viewsToClone = request.GetObjectsToClone();

            var done = 0;
            var todo = viewsToClone.Count();
            var cloneService = RecordService.CloneForParellelProcessing() as XrmRecordService;
            foreach (var viewToClone in viewsToClone)
            {
                controller.UpdateProgress(done, todo, $"Sharing saved dashboards {done}/{todo}");
                done++;
                try
                {
                    cloneService.ImpersonatingUserId = new Guid(viewToClone.OwningUserId);
                    var objectReference = new EntityReference(Entities.userform, new Guid(viewToClone.Id));
                    var userReference = new EntityReference(Entities.systemuser, new Guid(request.DashboardToUser.Id));
                    var accessRights = AccessRights.ReadAccess;
                    if(request.IncludeWriteAccess)
                    {
                        accessRights = accessRights | AccessRights.WriteAccess;
                    }
                    if(request.IncludeDeleteAccess)
                    {
                        accessRights = accessRights | AccessRights.DeleteAccess;
                    }
                    if (request.IncludeDeleteAccess)
                    {
                        accessRights = accessRights | AccessRights.ShareAccess;
                    }
                    if (request.IncludeAssignAccess)
                    {
                        accessRights = accessRights | AccessRights.AssignAccess;
                    }

                    // 4. Create and execute the GrantAccessRequest
                    var grantRequest = new GrantAccessRequest
                    {
                        Target = objectReference,
                        PrincipalAccess = new PrincipalAccess
                        {
                            Principal = userReference,
                            AccessMask = accessRights
                        }
                    };

                    cloneService.XrmService.Execute(grantRequest);
                }
                catch (Exception ex)
                {
                    response.AddResponseItem(new ShareSavedDashboardResponseItem(viewToClone.Name, ex));
                    response.NumberOfErrors++;
                }
                response.TotalRecordsProcessed++;
            }

            response.Message = "Dashboard Process Completed";
        }
    }
}