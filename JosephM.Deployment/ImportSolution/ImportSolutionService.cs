﻿using JosephM.Core.Service;
using JosephM.Deployment.DeployPackage;
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
            //just use the method in DeployPackageService to do the import
            var xrmRecordService = new XrmRecordService(request.Connection, ConnectionFactory);
            var service = new DeployPackageService(null);
            var importSolutionResponse = service.ImportSolutions(new[] { request.SolutionZip.FileName }, controller.Controller, xrmRecordService);
            response.LoadImportSolutionsResponse(importSolutionResponse);
            response.Connection = request.Connection;
            if (!importSolutionResponse.Success)
            {
                response.Message = $"There was an error importing the solution";
            }
            else
            {
                response.Message = $"The Solution Has Been Deployed Into {request.Connection}";
            }
        }
    }
}