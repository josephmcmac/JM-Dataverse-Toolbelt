using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Deployment.DataImport;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.Deployment.MigrateRecords
{
    public class MigrateRecordsDialog :
        ServiceRequestDialog
            <MigrateRecordsService, MigrateRecordsRequest,
                MigrateRecordsResponse, DataImportResponseItem>
    {
        public MigrateRecordsDialog(MigrateRecordsService service,
            IDialogController dialogController)
            : base(service, dialogController)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            CompletionMessage = "The Record Migration Has Completed";
            AddCompletionOption($"Open {Request.TargetConnection}", () =>
            {
                try
                {
                    ApplicationController.StartProcess(new XrmRecordService(Request.TargetConnection).WebUrl);
                }
                catch (Exception ex)
                {
                    ApplicationController.ThrowException(ex);
                }
            });
        }
    }
}