using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.Deployment.DataImport;

namespace JosephM.Deployment.CreatePackage
{
    [MyDescription("Create A Folder Containing A Solution File And Optionally Including A Set Of Records For Deploying Into A Target CRM Instance")]
    public class CreatePackageModule
        : ServiceRequestModule<CreatePackageDialog, CreatePackageService, CreatePackageRequest, CreatePackageResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";

        public override string MainOperationName { get { return "Create Package"; } }
    }
}