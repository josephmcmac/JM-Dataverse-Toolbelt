using JosephM.Application.Prism.Module.Dialog;

namespace JosephM.Application.Prism.Module.Crud
{
    public abstract class CrudModule<T> : DialogModule<T>
        where T : CrudDialog
    {
    }
}
