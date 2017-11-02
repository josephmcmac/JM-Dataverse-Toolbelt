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
            //todo should maybe do in base class
            AddOption("Plugins", "Manage Plugin Triggers", DialogCommand);
        }

        public override void RegisterTypes()
        {
        }

        public override void DialogCommand()
        {
            ApplicationController.RequestNavigate("Main", typeof(ManagePluginTriggersDialog), null);
        }
    }
}
