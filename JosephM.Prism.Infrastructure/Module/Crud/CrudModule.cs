using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Prism.Infrastructure.Module.Crud
{
    public abstract class CrudModule<T> : DialogModule<T>
        where T : CrudDialog
    {
    }
}
