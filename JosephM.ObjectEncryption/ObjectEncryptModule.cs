#region



#endregion

using JosephM.Prism.Infrastructure.Constants;
using JosephM.Prism.Infrastructure.Module;

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