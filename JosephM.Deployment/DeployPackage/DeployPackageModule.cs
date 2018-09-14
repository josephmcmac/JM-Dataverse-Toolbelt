using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.Deployment.DataImport;

namespace JosephM.Deployment.DeployPackage
{
    [MyDescription("Deploy A Solution Package Into A Target CRM instance")]
    public class DeployPackageModule
        : ServiceRequestModule<DeployPackageDialog, DeployPackageService, DeployPackageRequest, DeployPackageResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}