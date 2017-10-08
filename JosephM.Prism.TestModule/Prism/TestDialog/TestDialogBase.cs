using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Prism.Infrastructure.Dialog;

namespace JosephM.Prism.TestModule.Prism.TestDialog
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