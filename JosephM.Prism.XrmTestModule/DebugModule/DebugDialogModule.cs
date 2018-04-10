using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Core.Attributes;

namespace JosephM.Prism.XrmTestModule.DebugModule
{
    [MyDescription("A Fake Dialog Module For Testing")]
    public class DebugDialogModule :
        ServiceRequestModule
            <DebugDialog, DebugDialogService, DebugDialogRequest, DebugDialogResponse, DebugDialogResponseItem>
    {
    }
}