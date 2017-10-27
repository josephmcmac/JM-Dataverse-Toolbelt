#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Xrm.XrmRecord;

#endregion

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
        }
    }
}