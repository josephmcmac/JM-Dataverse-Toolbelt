using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.XrmModule.Crud.BulkWorkflow
{
    public class BulkWorkflowDialog :
        ServiceRequestDialog<BulkWorkflowService, BulkWorkflowRequest, BulkWorkflowResponse, BulkWorkflowResponseItem>
    {
        public BulkWorkflowDialog(XrmRecordService recordService, IDialogController dialogController, BulkWorkflowRequest request, Action onClose)
            : base(new BulkWorkflowService(recordService), dialogController, recordService, request, onClose)
        {
            
        }

        public override bool DisplayResponseDuringServiceRequestExecution => true;
    }
}