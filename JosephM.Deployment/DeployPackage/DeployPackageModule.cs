#region

using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Deployment.DeployPackage
{
    [MyDescription("Deploy A Solution Package Into A Target CRM instance")]
    public class DeployPackageModule
        : ServiceRequestModule<DeployPackageDialog, DeployPackageService, DeployPackageRequest, ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}