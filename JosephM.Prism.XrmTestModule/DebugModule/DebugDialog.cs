using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Prism.XrmTestModule.DebugModule
{
    public class DebugDialog :
        ServiceRequestDialog<DebugDialogService, DebugDialogRequest, DebugDialogResponse, DebugDialogResponseItem>
    {
        public DebugDialog(IDialogController dialogController, DebugDialogService dialogService, XrmRecordService lookupService)
            : base(dialogService, dialogController, lookupService)
        {
        }
    }
}