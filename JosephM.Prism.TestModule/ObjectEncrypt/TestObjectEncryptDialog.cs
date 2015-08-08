
using JosephM.ObjectEncryption;
using JosephM.Prism.TestModule.SearchModule;
using JosephM.Record.Application.Dialog;

namespace JosephM.Prism.TestModule.ObjectEncrypt
{
    public class TestObjectEncryptDialog : ObjectEncryptDialog<TestClassToEncrypt>
    {
        public TestObjectEncryptDialog(IDialogController dialogController) : base(dialogController)
        {
        }
    }
}
