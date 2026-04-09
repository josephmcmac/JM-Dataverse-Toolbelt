using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.SavedViewsExport.CloneSavedViews
{
    public class CloneSavedViewsDialog :
        ServiceRequestDialog<CloneSavedViewsService, CloneSavedViewsRequest, CloneSavedViewsResponse, CloneSavedViewsResponseItem>
    {
        public CloneSavedViewsDialog(XrmRecordService recordService, IDialogController dialogController, CloneSavedViewsRequest request, Action onClose)
            : base(new CloneSavedViewsService(recordService), dialogController, recordService, request, onClose)
        {

        }

        public override bool DisplayResponseDuringServiceRequestExecution => true;
    }
}