using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;

namespace JosephM.TestModule.TestGridOnly
{
    public class TestGridOnlyDialog :
        ServiceRequestDialog<TestGridOnlyService, TestGridOnlyRequest, TestGridOnlyResponse, TestGridOnlyResponseItem>
    {
        public TestGridOnlyDialog(IDialogController dialogController)
            : base(new TestGridOnlyService(), dialogController, FakeRecordService.Get())
        {
            
        }
    }
}