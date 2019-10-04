using JosephM.Core.Service;
using JosephM.Deployment.DeployPackage;
using JosephM.Record.Xrm.XrmRecord;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.ImportSolution
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
            //just use the method in DeployPackageService to do the import
            var xrmRecordService = new XrmRecordService(request.Connection, ConnectionFactory);
            var service = new DeployPackageService(null);
            var importItems = service.ImportSolutions(new[] { request.SolutionZip.FileName }, controller.Controller, xrmRecordService);
            response.AddResponseItems(importItems.Select(it => new ImportSolutionResponseItem(it)));
            response.Connection = request.Connection;
            response.Message = $"The Solution Has Been Deployed Into {request.Connection}";
        }
    }
}