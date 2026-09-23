using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.UserSavedObjectsUtility.Charts.Clone
{
    public class CloneSavedChartsDialog :
        ServiceRequestDialog<CloneSavedChartsService, CloneSavedChartsRequest, CloneSavedChartsResponse, CloneSavedChartsResponseItem>
    {
        public CloneSavedChartsDialog(XrmRecordService recordService, IDialogController dialogController, CloneSavedChartsRequest request, Action onClose)
            : base(new CloneSavedChartsService(recordService), dialogController, recordService, request, onClose)
        {

        }

        public override bool DisplayResponseDuringServiceRequestExecution => true;
    }
}