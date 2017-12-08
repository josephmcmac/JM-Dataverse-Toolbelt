using JosephM.Core.Attributes;
using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Prism.TestModule.Prism.TestDialog
{
    [MyDescription("Just a fake dialog for testing")]
    public class TestDialogModule :
        ServiceRequestModule
            <TestDialog, TestDialogService, TestDialogRequest, TestDialogResponse, TestDialogResponseItem>
    {
    }
}