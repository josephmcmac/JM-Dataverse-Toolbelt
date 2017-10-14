using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;
using JosephM.Prism.Infrastructure.Module.Crud;

namespace JosephM.Prism.TestModule.Prism.TestCrud
{
    public class TestCrudDialog : CrudDialog
    {
        public TestCrudDialog(IDialogController dialogController)
            : base(dialogController, FakeRecordService.Get())
        {

        }
    }
}
