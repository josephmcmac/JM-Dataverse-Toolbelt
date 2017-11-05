#region

using JosephM.Core.Service;
using JosephM.Prism.Infrastructure.Module;

#endregion

namespace JosephM.Deployment.CreatePackage
{
    public class CreatePackageModule
        : ServiceRequestModule<CreatePackageDialog, CreatePackageService, CreatePackageRequest, ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}