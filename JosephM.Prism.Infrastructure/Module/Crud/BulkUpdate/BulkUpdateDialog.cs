using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.IService;
using System;

namespace JosephM.Application.Desktop.Module.Crud.BulkUpdate
{
    public class BulkUpdateDialog :
        ServiceRequestDialog<BulkUpdateService, BulkUpdateRequest, BulkUpdateResponse, BulkUpdateResponseItem>
    {
        public BulkUpdateDialog(IRecordService recordService, IDialogController dialogController, BulkUpdateRequest request, Action onClose)
            : base(new BulkUpdateService(recordService), dialogController, recordService, request, onClose)
        {
        }

        protected override void CompleteDialogExtention()
        {
            CompletionMessage = "Updates Completed";
            base.CompleteDialogExtention();
        }
    }
}