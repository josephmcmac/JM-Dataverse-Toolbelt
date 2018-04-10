using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Core.Attributes;

namespace JosephM.Prism.TestModule.Prism.TestDialog
{
    [MyDescription("Just a fake dialog for testing")]
    public class TestDialogModule :
        ServiceRequestModule
            <TestDialog, TestDialogService, TestDialogRequest, TestDialogResponse, TestDialogResponseItem>
    {
    }
}