#region

using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;

#endregion

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