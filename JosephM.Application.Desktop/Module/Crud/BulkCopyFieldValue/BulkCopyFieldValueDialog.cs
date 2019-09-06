using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.IService;
using System;

namespace JosephM.Application.Desktop.Module.Crud.BulkCopyFieldValue
{
    public class BulkCopyFieldValueDialog :
        ServiceRequestDialog<BulkCopyFieldValueService, BulkCopyFieldValueRequest, BulkCopyFieldValueResponse, BulkCopyFieldValueResponseItem>
    {
        public BulkCopyFieldValueDialog(IRecordService recordService, IDialogController dialogController, BulkCopyFieldValueRequest request, Action onClose)
            : base(new BulkCopyFieldValueService(recordService), dialogController, recordService, request, onClose)
        {
        }

        public override bool DisplayResponseDuringServiceRequestExecution => true;
    }
}