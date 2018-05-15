using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.IService;
using System;

namespace JosephM.Application.Desktop.Module.Crud.BulkReplace
{
    public class BulkReplaceDialog :
        ServiceRequestDialog<BulkReplaceService, BulkReplaceRequest, BulkReplaceResponse, BulkReplaceResponseItem>
    {
        public BulkReplaceDialog(IRecordService recordService, IDialogController dialogController, BulkReplaceRequest request, Action onClose)
            : base(new BulkReplaceService(recordService), dialogController, recordService, request, onClose)
        {
        }

        protected override void CompleteDialogExtention()
        {
            CompletionMessage = "Replaces Completed";
            base.CompleteDialogExtention();
        }
    }
}