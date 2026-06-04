using JosephM.Core.Extentions;
using JosephM.Core.Service;
using JosephM.Record.Extentions;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.Security.Principal;
using static JosephM.Xrm.Schema.Fields;

namespace JosephM.SavedViewsExport.ShareSavedViews
{
    public class ShareSavedViewsService :
        ServiceBase<ShareSavedViewsRequest, ShareSavedViewsResponse, ShareSavedViewsResponseItem>
    {
        public XrmRecordService RecordService { get; set; }
        public ShareSavedViewsService(XrmRecordService recordService)
        {
            RecordService = recordService;
        }

        public override void ExecuteExtention(ShareSavedViewsRequest request, ShareSavedViewsResponse response,
            ServiceRequestController controller)
        {
            var countToUpdate = request.RecordCount;
            controller.UpdateProgress(0, countToUpdate, "Sharing Views");
            var viewsToClone = request.GetViewsToClone();

            var done = 0;
            var todo = viewsToClone.Count();
            var cloneService = RecordService.CloneForParellelProcessing() as XrmRecordService;
            foreach (var viewToClone in viewsToClone)
            {
                controller.UpdateProgress(done, todo, $"Sharing saved views {done}/{todo}");
                done++;
                try
                {
                    cloneService.ImpersonatingUserId = new Guid(viewToClone.OwningUserId);
                    var viewReference = new EntityReference("userquery", new Guid(viewToClone.Id));
                    var userReference = new EntityReference(Entities.systemuser, new Guid(request.ShareToUser.Id));
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
                        Target = viewReference,
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
                    response.AddResponseItem(new ShareSavedViewsResponseItem(viewToClone.Name, ex));
                    response.NumberOfErrors++;
                }
                response.TotalRecordsProcessed++;
            }

            response.Message = "Share Process Completed";
        }
    }
}