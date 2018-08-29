using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using System;

namespace JosephM.Deployment.ImportCsvs
{
    [RequiresConnection]
    public class ImportCsvsDialog :
        ServiceRequestDialog
            <ImportCsvsService, ImportCsvsRequest,
                ImportCsvsResponse, ImportCsvsResponseItem>
    {
        public ImportCsvsDialog(ImportCsvsService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }

        protected override void CompleteDialogExtention()
        {
            base.CompleteDialogExtention();
            CompletionMessage = "The Import Process Has Completed";
            AddCompletionOption($"Open {Service?.XrmRecordService?.XrmRecordConfiguration?.OrganizationUniqueName}", () =>
            {
                try
                {
                    ApplicationController.StartProcess(Service?.XrmRecordService?.WebUrl);
                }
                catch (Exception ex)
                {
                    ApplicationController.ThrowException(ex);
                }
            });
        }
    }
}