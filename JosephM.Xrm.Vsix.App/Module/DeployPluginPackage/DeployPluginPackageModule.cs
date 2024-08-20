using JosephM.Application.Modules;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.Vsix.Module.DeployPluginPackage
{
    [DeployPluginPackageMenuItemVisible]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class DeployPluginPackageModule : OptionActionModule
    {
        public override string MainOperationName => "Update Plugin Package";

        public override string MenuGroup => "Plugins";

        public override void DialogCommand()
        {
            ApplicationController.NavigateTo(typeof(DeployPluginPackageDialog), null);
        }
    }
}
