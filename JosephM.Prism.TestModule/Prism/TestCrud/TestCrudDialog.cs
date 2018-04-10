using JosephM.Application.Prism.Module.Crud;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;

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
