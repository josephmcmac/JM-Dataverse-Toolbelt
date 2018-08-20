using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Deployment;

namespace JosephM.Xrm.Vsix.Module.ImportRecords
{
    public class ImportRecordsDialog
        : ServiceRequestDialog<ImportRecordsService, ImportRecordsRequest, ImportRecordsResponse, DataImportResponseItem>
    {
        public ImportRecordsDialog(ImportRecordsService service, IDialogController dialogController)
            : base(service, dialogController)
        {
        }
    }
}
