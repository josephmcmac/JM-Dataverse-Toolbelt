using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module.Crud;
using JosephM.Prism.XrmModule.XrmConnection;

namespace JosephM.Prism.XrmModule.Forms
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class XrmCrudModule : CrudModule<XrmCrudDialog>
    {
    }
}
