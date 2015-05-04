using JosephM.Prism.Infrastructure.Dialog;
using JosephM.Record.Application.Dialog;

namespace JosephM.Prism.TestModule.Prism.TestDialog
{
    public class TestDialog :
        ServiceRequestDialog<TestDialogService, TestDialogRequest, TestDialogResponse, TestDialogResponseItem>
    {
        public TestDialog(IDialogController dialogController)
            : base(new TestDialogService(), dialogController)
        {
        }
    }
}