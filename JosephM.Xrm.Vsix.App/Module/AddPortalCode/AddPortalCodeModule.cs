using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.XrmConnection;

namespace JosephM.Xrm.Vsix.Module.AddPortalCode
{
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(XrmConnectionModule))]
    public class AddPortalCodeModule : ServiceRequestModule<AddPortalCodeDialog, AddPortalCodeService, AddPortalCodeRequest, AddPortalCodeResponse, AddPortalCodeResponseItem>
    {
    }
}
