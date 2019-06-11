using JosephM.Application;
using JosephM.Application.Modules;
using JosephM.XrmModule.XrmConnection;
using JosephM.Xrm.Vsix.Application;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using System;
using JosephM.Application.Desktop.Module.ServiceRequest;

namespace JosephM.Xrm.Vsix.Module.DeployWebResource
{
    [DeployWebResourceMenuItemVisible]
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class DeployWebResourceModule : ServiceRequestModule<DeployWebResourceDialog, DeployWebResourceService, DeployWebResourceRequest, DeployWebResourceResponse, DeployWebResourceResponseItem>
    {
    }
}
