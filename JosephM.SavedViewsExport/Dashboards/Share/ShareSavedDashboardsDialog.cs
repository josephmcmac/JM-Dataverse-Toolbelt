using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.UserSavedObjectsUtility.Dashboards.Share
{
    public class ShareSavedDashboardDialog :
        ServiceRequestDialog<ShareSavedDashboardService, ShareSavedDashboardRequest, ShareSavedDashboardResponse, ShareSavedDashboardResponseItem>
    {
        public ShareSavedDashboardDialog(XrmRecordService recordService, IDialogController dialogController, ShareSavedDashboardRequest request, Action onClose)
            : base(new ShareSavedDashboardService(recordService), dialogController, recordService, request, onClose)
        {

        }

        public override bool DisplayResponseDuringServiceRequestExecution => true;
    }
}