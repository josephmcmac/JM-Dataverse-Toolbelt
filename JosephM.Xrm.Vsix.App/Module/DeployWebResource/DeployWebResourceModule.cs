using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.Vsix.Module.DeployWebResource
{
    [DeployWebResourceMenuItemVisible]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class DeployWebResourceModule : ServiceRequestModule<DeployWebResourceDialog, DeployWebResourceService, DeployWebResourceRequest, DeployWebResourceResponse, DeployWebResourceResponseItem>
    {
    }
}
