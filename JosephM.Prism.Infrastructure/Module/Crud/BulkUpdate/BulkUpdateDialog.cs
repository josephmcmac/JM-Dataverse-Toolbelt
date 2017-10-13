using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.IService;

namespace JosephM.Prism.Infrastructure.Module.Crud.BulkUpdate
{
    public class BulkUpdateDialog :
        ServiceRequestDialog<BulkUpdateService, BulkUpdateRequest, BulkUpdateResponse, BulkUpdateResponseItem>
    {
        public BulkUpdateDialog(IRecordService recordService, IDialogController dialogController, BulkUpdateRequest request)
            : base(new BulkUpdateService(recordService), dialogController, recordService, request)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
        }
    }
}