using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.Xrm.DataImportExport.Import;

namespace JosephM.Deployment.CreatePackage
{
    [MyDescription("Create a folder containing a solution file and optionally data for deployment")]
    public class CreatePackageModule
        : ServiceRequestModule<CreatePackageDialog, CreatePackageService, CreatePackageRequest, CreatePackageResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Solution Deployment";

        public override string MainOperationName { get { return "Create Package"; } }
    }
}