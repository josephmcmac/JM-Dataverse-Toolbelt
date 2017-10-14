using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module.Crud;
using JosephM.Prism.XrmModule.XrmConnection;

namespace JosephM.Prism.XrmModule.Crud
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class XrmCrudModule : CrudModule<XrmCrudDialog>
    {
        protected override string MainOperationName
        {
            get { return "Browse/Update Data"; }
        }
    }
}
