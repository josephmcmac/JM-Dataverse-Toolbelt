using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.Extentions;

namespace JosephM.RecordCounts
{
    public class RecordCountsDialog :
        ServiceRequestDialog
            <RecordCountsService, RecordCountsRequest, RecordCountsResponse,
                RecordCountsResponseItem>
    {
        public RecordCountsDialog(RecordCountsService service, IDialogController dialogController,
            XrmRecordService recordService)
            : base(service, dialogController, recordService)
        {
            this.AddRedirectToConnectionEntryWhenNotConnected(recordService);
        }
    }
}