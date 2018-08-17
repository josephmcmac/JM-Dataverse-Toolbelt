using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.XrmModule.Extentions;

namespace JosephM.Deployment.ImportCsvs
{
    public class ImportCsvsDialog :
        ServiceRequestDialog
            <ImportCsvsService, ImportCsvsRequest,
                ImportCsvsResponse, ImportCsvsResponseItem>
    {
        public ImportCsvsDialog(ImportCsvsService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
            this.AddRedirectToConnectionEntryWhenNotConnected(lookupService);
        }
    }
}