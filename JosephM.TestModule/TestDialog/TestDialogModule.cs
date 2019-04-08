using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;

namespace JosephM.TestModule.TestDialog
{
    [MyDescription("Just a fake dialog for testing")]
    public class TestDialogModule :
        ServiceRequestModule
            <TestDialog, TestDialogService, TestDialogRequest, TestDialogResponse, TestDialogResponseItem>
    {
    }
}