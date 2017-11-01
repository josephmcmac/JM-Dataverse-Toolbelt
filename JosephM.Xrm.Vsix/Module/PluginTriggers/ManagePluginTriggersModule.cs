using JosephM.Application.Modules;
using JosephM.Prism.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Module.PackageSettings;

namespace JosephM.Xrm.Vsix.Module.PluginTriggers
{
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class ManagePluginTriggersModule : ActionModuleBase
    {
        public override void InitialiseModule()
        {
        }

        public override void RegisterTypes()
        {
        }

        public override void DialogCommand()
        {
            //todo verify the package settings resolved when dialog created
            ApplicationController.RequestNavigate("Main", typeof(ManagePluginTriggersDialog), null);
        }
    }
}
