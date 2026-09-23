using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.UserSavedObjectsUtility.Dashboards.Clone
{
    public class CloneSavedDashboardsDialog :
        ServiceRequestDialog<CloneSavedDashboardsService, CloneSavedDashboardsRequest, CloneSavedDashboardsResponse, CloneSavedDashboardsResponseItem>
    {
        public CloneSavedDashboardsDialog(XrmRecordService recordService, IDialogController dialogController, CloneSavedDashboardsRequest request, Action onClose)
            : base(new CloneSavedDashboardsService(recordService), dialogController, recordService, request, onClose)
        {

        }

        public override bool DisplayResponseDuringServiceRequestExecution => true;
    }
}