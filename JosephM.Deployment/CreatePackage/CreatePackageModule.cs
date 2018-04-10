using JosephM.Application.Prism.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.Core.Service;


namespace JosephM.Deployment.CreatePackage
{
    [MyDescription("Create A Folder Containing A Solution File And Optionally Including A Set Of Records For Deploying Into A Target CRM Instance")]
    public class CreatePackageModule
        : ServiceRequestModule<CreatePackageDialog, CreatePackageService, CreatePackageRequest, ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}