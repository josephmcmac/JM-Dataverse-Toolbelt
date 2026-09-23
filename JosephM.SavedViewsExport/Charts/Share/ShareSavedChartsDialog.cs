using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.UserSavedObjectsUtility.Charts.Share
{
    public class ShareSavedChartDialog :
        ServiceRequestDialog<ShareSavedChartService, ShareSavedChartRequest, ShareSavedChartResponse, ShareSavedChartResponseItem>
    {
        public ShareSavedChartDialog(XrmRecordService recordService, IDialogController dialogController, ShareSavedChartRequest request, Action onClose)
            : base(new ShareSavedChartService(recordService), dialogController, recordService, request, onClose)
        {

        }

        public override bool DisplayResponseDuringServiceRequestExecution => true;
    }
}