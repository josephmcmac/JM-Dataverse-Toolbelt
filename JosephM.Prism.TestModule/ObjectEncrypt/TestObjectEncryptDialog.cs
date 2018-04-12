using JosephM.Application.ViewModel.Dialog;

using JosephM.ObjectEncryption;

namespace JosephM.TestModule.ObjectEncrypt
{
    public class TestObjectEncryptDialog : ObjectEncryptDialog<TestClassToEncrypt>
    {
        public TestObjectEncryptDialog(IDialogController dialogController) : base(dialogController)
        {
        }
    }
}
