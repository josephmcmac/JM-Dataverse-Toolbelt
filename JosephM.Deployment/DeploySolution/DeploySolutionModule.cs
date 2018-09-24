using JosephM.Application.Desktop.Module.ServiceRequest;
using JosephM.Core.Attributes;
using JosephM.Deployment.DataImport;

namespace JosephM.Deployment.DeploySolution
{
    [MyDescription("Deploy A Solution From One Instance into Another")]
    public class DeploySolutionModule
        : ServiceRequestModule<DeploySolutionDialog, DeploySolutionService, DeploySolutionRequest, DeploySolutionResponse, DataImportResponseItem>
    {
        public override string MenuGroup => "Deployment";
    }
}