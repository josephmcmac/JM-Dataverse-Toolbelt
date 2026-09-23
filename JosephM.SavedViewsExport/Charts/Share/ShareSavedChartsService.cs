using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;

namespace JosephM.UserSavedObjectsUtility.Charts.Share
{
    public class ShareSavedChartService :
        ServiceBase<ShareSavedChartRequest, ShareSavedChartResponse, ShareSavedChartResponseItem>
    {
        public XrmRecordService RecordService { get; set; }
        public ShareSavedChartService(XrmRecordService recordService)
        {
            RecordService = recordService;
        }

        public override void ExecuteExtention(ShareSavedChartRequest request, ShareSavedChartResponse response,
            ServiceRequestController controller)
        {
            var countToUpdate = request.RecordCount;
            controller.UpdateProgress(0, countToUpdate, "Sharing saved charts");
            var objectsToShare = request.GetObjectsToClone();

            var done = 0;
            var todo = objectsToShare.Count();
            var cloneService = RecordService.CloneForParellelProcessing() as XrmRecordService;
            foreach (var objectToShare in objectsToShare)
            {
                controller.UpdateProgress(done, todo, $"Sharing saved charts {done}/{todo}");
                done++;
                try
                {
                    cloneService.ImpersonatingUserId = new Guid(objectToShare.OwningUserId);
                    var objectReference = new EntityReference(Entities.userqueryvisualization, new Guid(objectToShare.Id));
                    var userReference = new EntityReference(Entities.systemuser, new Guid(request.ChartToUser.Id));
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
                    response.AddResponseItem(new ShareSavedChartResponseItem(objectToShare.Name, ex));
                    response.NumberOfErrors++;
                }
                response.TotalRecordsProcessed++;
            }

            response.Message = "Share process completed";
        }
    }
}