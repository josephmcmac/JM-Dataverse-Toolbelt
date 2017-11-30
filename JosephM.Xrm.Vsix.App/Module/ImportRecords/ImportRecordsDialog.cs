using JosephM.Application.ViewModel.Dialog;
using JosephM.Deployment;
using JosephM.Prism.Infrastructure.Dialog;

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
