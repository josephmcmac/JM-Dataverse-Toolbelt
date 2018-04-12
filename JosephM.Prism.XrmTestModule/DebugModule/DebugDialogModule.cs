using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;

namespace JosephM.XrmTestModule.DebugModule
{
    [MyDescription("A Fake Dialog Module For Testing")]
    public class DebugDialogModule :
        ServiceRequestModule
            <DebugDialog, DebugDialogService, DebugDialogRequest, DebugDialogResponse, DebugDialogResponseItem>
    {
    }
}