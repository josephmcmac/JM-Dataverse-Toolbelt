#region

using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Deployment.DeployPackage
{
    public class DeployPackageModule
        : ServiceRequestModule<DeployPackageDialog, DeployPackageService, DeployPackageRequest, ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}