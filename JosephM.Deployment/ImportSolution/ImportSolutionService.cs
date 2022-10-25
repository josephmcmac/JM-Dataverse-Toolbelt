using JosephM.Core.Service;
using JosephM.Deployment.DeployPackage;
using JosephM.Deployment.SolutionsImport;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;

namespace JosephM.Deployment.ImportSolution
{
    public class ImportSolutionService :
        ServiceBase<ImportSolutionRequest, ImportSolutionResponse, ImportSolutionResponseItem>
    {
        public ImportSolutionService(IOrganizationConnectionFactory connectionFactory)
        {
            ConnectionFactory = connectionFactory;
        }

        private IOrganizationConnectionFactory ConnectionFactory { get; }

        public override void ExecuteExtention(ImportSolutionRequest request, ImportSolutionResponse response,
            ServiceRequestController controller)
        {
            var xrmRecordService = new XrmRecordService(request.TargetConnection, ConnectionFactory);
            var service = new ImportSolutionsService(xrmRecordService);

            var importSolutionResponse = service.ImportSolutions(new ImportSolutionsRequest
            {
                Items = new[] { request }
            }, controller.Controller);
            response.LoadImportSolutionsResponse(importSolutionResponse);
            response.Connection = request.TargetConnection;
            if (!importSolutionResponse.Success)
            {
                response.Message = $"There was an error importing the solution";
            }
            else
            {
                response.Message = $"The Solution Has Been Deployed Into {request.TargetConnection}";
            }
        }
    }
}