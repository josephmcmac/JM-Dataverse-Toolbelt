using JosephM.Application.ViewModel.Dialog;
using JosephM.Core.Extentions;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.RecordCounts.Exporter
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
        }
    }
}