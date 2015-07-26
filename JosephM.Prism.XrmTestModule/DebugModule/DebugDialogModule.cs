using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Prism.XrmTestModule.DebugModule
{
    public class DebugDialogModule :
        ServiceRequestModule
            <DebugDialog, DebugDialogService, DebugDialogRequest, DebugDialogResponse, DebugDialogResponseItem>
    {
    }
}