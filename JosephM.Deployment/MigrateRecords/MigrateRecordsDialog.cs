using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;

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
    }
}