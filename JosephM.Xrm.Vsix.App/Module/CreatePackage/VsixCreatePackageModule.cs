using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Application.Modules;
using JosephM.Deployment.CreatePackage;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.Vsix.Module.PackageSettings;
using JosephM.XrmModule.SavedXrmConnections;

namespace JosephM.Xrm.Vsix.Module.CreatePackage
{
    [DependantModule(typeof(XrmPackageSettingsModule))]
    [DependantModule(typeof(SavedXrmConnectionsModule))]
    public class VsixCreatePackageModule
                : ServiceRequestModule<VsixCreatePackageDialog, CreatePackageService, CreatePackageRequest, CreatePackageResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}
