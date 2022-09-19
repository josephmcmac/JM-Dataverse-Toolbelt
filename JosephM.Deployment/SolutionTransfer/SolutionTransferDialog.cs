using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Deployment.SolutionTransfer
{
    public class SolutionTransferDialog :
        ServiceRequestDialog
            <SolutionTransferService, SolutionTransferRequest,
                SolutionTransferResponse, SolutionTransferResponseItem>
    {
        public SolutionTransferDialog(SolutionTransferService service,
            IDialogController dialogController, XrmRecordService lookupService)
            : base(service, dialogController, lookupService)
        {
        }
    }
}