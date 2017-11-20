#region

using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Xrm.XrmRecord;

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