#region

using JosephM.Application.Modules;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.XrmConnection;

#endregion

namespace JosephM.Prism.XrmModule.Xrm
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class XrmModuleModule : ModuleBase
    {
        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<XrmMaintainViewModel>();
            RegisterTypeForNavigation<XrmCreateViewModel>();
            //RegisterType<XrmFormController>();
            //RegisterType<XrmFormService>();
        }

        public override void InitialiseModule()
        {

        }
    }
}