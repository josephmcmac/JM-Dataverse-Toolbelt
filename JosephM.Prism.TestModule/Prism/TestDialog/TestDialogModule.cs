using JosephM.Prism.Infrastructure.Module;

namespace JosephM.Prism.TestModule.Prism.TestDialog
{
    public class TestDialogModule :
        ServiceRequestModule
            <TestDialog, TestDialogService, TestDialogRequest, TestDialogResponse, TestDialogResponseItem>
    {
    }
}