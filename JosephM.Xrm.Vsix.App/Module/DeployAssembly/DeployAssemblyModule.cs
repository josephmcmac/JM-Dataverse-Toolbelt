using JosephM.Application.Modules;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.XrmConnection;

namespace JosephM.Xrm.Vsix.Module.DeployAssembly
{
    [MenuItemVisibleForPluginProject]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class DeployAssemblyModule : OptionActionModule
    {
        public override string MainOperationName => "Deploy Assembly";

        public override string MenuGroup => "Plugins";

        public override void DialogCommand()
        {
            ApplicationController.NavigateTo(typeof(DeployAssemblyDialog), null);
        }
    }
}