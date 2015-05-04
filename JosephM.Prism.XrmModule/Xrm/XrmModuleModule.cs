#region

using JosephM.Prism.Infrastructure.Attributes;
using JosephM.Prism.Infrastructure.Module;
using JosephM.Prism.XrmModule.XrmConnection;

#endregion

namespace JosephM.Prism.XrmModule.Xrm
{
    [DependantModule(typeof(XrmConnectionModule))]
    public class XrmModuleModule : PrismModuleBase
    {
        public override void RegisterTypes()
        {
            RegisterTypeForNavigation<XrmMaintainViewModel>();
            RegisterTypeForNavigation<XrmCreateViewModel>();
            RegisterType<XrmFormController>();
            RegisterType<XrmFormService>();
        }

        public override void InitialiseModule()
        {

        }
    }
}