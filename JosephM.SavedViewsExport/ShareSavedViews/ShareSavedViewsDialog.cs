using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.SavedViewsExport.ShareSavedViews
{
    public class ShareSavedViewsDialog :
        ServiceRequestDialog<ShareSavedViewsService, ShareSavedViewsRequest, ShareSavedViewsResponse, ShareSavedViewsResponseItem>
    {
        public ShareSavedViewsDialog(XrmRecordService recordService, IDialogController dialogController, ShareSavedViewsRequest request, Action onClose)
            : base(new ShareSavedViewsService(recordService), dialogController, recordService, request, onClose)
        {

        }

        public override bool DisplayResponseDuringServiceRequestExecution => true;
    }
}