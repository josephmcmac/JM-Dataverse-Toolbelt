using JosephM.Application.Desktop.Module.Dialog;

namespace JosephM.Application.Desktop.Module.Crud
{
    public abstract class CrudModule<T> : DialogModule<T>
        where T : CrudDialog
    {
    }
}
