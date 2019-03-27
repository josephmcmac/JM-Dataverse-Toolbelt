using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.IService;
using System;

namespace JosephM.Application.Desktop.Module.Crud.BulkDelete
{
    public class BulkDeleteDialog :
        ServiceRequestDialog<BulkDeleteService, BulkDeleteRequest, BulkDeleteResponse, BulkDeleteResponseItem>
    {
        public BulkDeleteDialog(IRecordService recordService, IDialogController dialogController, BulkDeleteRequest request, Action onClose)
            : base(new BulkDeleteService(recordService), dialogController, recordService, request, onClose)
        {
        }
    }
}