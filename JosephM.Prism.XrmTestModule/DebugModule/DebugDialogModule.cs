using JosephM.Core.Attributes;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Prism.XrmTestModule.DebugModule
{
    [MyDescription("A Fake Dialog Module For Testing")]
    public class DebugDialogModule :
        ServiceRequestModule
            <DebugDialog, DebugDialogService, DebugDialogRequest, DebugDialogResponse, DebugDialogResponseItem>
    {
    }
}