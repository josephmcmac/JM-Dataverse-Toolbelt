

using JosephM.Application.Prism.Module.Dialog;

namespace JosephM.ObjectEncryption
{
    public class ObjectEncryptModule<TDialog, TTypeToEnter> : DialogModule<TDialog>
        where TDialog : ObjectEncryptDialog<TTypeToEnter>
        where TTypeToEnter : new()
    {
        public override string MainOperationName
        {
            get { return string.Format("Encrypt {0}", typeof(TTypeToEnter).Name); }
        }
    }
}