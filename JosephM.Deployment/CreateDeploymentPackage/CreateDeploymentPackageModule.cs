#region

using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Deployment.CreateDeploymentPackage
{
    public class CreateDeploymentPackageModule
        : ServiceRequestModule<CreateDeploymentPackageDialog, CreateDeploymentPackageService, CreateDeploymentPackageRequest, ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}