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

        protected override void ProcessCompletionExtention()
        {
            if (!Response.RecordCountsFileName.IsNullOrWhiteSpace())
                AddCompletionOption("Open Record Counts", OpenRecordCountsFile);
            if (!Response.RecordCountsByOwnerFileName.IsNullOrWhiteSpace())
                AddCompletionOption("Open Record Counts By Owner", OpenRecordCountsByOwnerFile);
            AddCompletionOption("Open Folder", OpenFolder);
            CompletionMessage = "Document Successfully Generated";
        }

        public void OpenFolder()
        {
            ApplicationController.StartProcess("explorer", Response.Folder);
        }

        public void OpenRecordCountsFile()
        {
            ApplicationController.StartProcess(Response.RecordCountsFileNameQualified);
        }

        public void OpenRecordCountsByOwnerFile()
        {
            ApplicationController.StartProcess(Response.RecordCountsByOwnerFileNameQualified);
        }
    }
}