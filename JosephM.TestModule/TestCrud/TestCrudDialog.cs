using JosephM.Application.Desktop.Module.Crud;
using JosephM.Application.ViewModel.Dialog;
using JosephM.Application.ViewModel.Fakes;

namespace JosephM.TestModule.TestCrud
{
    public class TestCrudDialog : CrudDialog
    {
        public TestCrudDialog(IDialogController dialogController)
            : base(dialogController, FakeRecordService.Get())
        {

        }
    }
}
