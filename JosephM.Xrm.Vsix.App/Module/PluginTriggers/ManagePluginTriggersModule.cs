using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.XrmConnection;

namespace JosephM.Xrm.Vsix.Module.PluginTriggers
{
    [MenuItemVisibleForPluginProject]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class ManagePluginTriggersModule : ServiceRequestModule<ManagePluginTriggersDialog, ManagePluginTriggersService, ManagePluginTriggersRequest, ManagePluginTriggersResponse, ManagePluginTriggersResponseItem>
    {
        public override string MenuGroup => "Plugins";

    }
}

