using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Application.ViewModel.Shared;

namespace JosephM.TestModule.TestDialog
{
    public class TestDialog :
        ServiceRequestDialog<TestDialogService, TestDialogRequest, TestDialogResponse, TestDialogResponseItem>
    {
        public TestDialog(IDialogController dialogController)
            : base(new TestDialogService(), dialogController, FakeRecordService.Get())
        {
            
        }
    }
}