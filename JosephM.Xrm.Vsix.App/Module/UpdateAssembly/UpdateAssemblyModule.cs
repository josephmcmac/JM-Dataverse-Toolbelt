using JosephM.Application.Modules;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.Vsix.Module.UpdateAssembly
{
    [MenuItemVisibleForPluginProject]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class UpdateAssemblyModule : OptionActionModule
    {
        public override string MainOperationName => "Update Assembly";

        public override string MenuGroup => "Plugins";

        public override void DialogCommand()
        {
            ApplicationController.NavigateTo(typeof(UpdateAssemblyDialog), null);
        }
    }
}
