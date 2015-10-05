using JosephM.Application.ViewModel.Dialog;
using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Xrm.XrmRecord;

namespace JosephM.Prism.XrmTestModule.DebugModule
{
    public class DebugDialog :
        ServiceRequestDialog<DebugDialogService, DebugDialogRequest, DebugDialogResponse, DebugDialogResponseItem>
    {
        public DebugDialog(IDialogController dialogController, DebugDialogService dialogService)
            : base(dialogService, dialogController)
        {
        }
    }
}