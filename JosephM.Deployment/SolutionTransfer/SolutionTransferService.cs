using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.SolutionsImport;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
using System;

namespace JosephM.Deployment.SolutionTransfer
{
    public class SolutionTransferService :
        ServiceBase<SolutionTransferRequest, SolutionTransferResponse, SolutionTransferResponseItem>
    {
        public SolutionTransferService(IOrganizationConnectionFactory connectionFactory)
        {
            ConnectionFactory = connectionFactory;
        }

        private IOrganizationConnectionFactory ConnectionFactory { get; }

        public override void ExecuteExtention(SolutionTransferRequest request, SolutionTransferResponse response,
            ServiceRequestController controller)
        {
            DeploySolution(request, controller.Controller, response);
        }

        private void DeploySolution(SolutionTransferRequest request, LogController controller, SolutionTransferResponse response)
        {
            var tasksDone = 0;
            var totalTasks = 4;

            var sourceXrmRecordService = new XrmRecordService(request.SourceConnection, ConnectionFactory);
            var service = sourceXrmRecordService.XrmService;
            var solution = service.Retrieve(Entities.solution, new Guid(request.Solution.Id));
            tasksDone++;
            if (solution.GetStringField(Fields.solution_.version) != request.SourceVersionForRelease)
            {
                controller.UpdateProgress(tasksDone, totalTasks, "Setting Release Version " + request.SourceVersionForRelease);
                solution.SetField(Fields.solution_.version, request.SourceVersionForRelease);
                service.Update(solution, new[] { Fields.solution_.version });
            }
            tasksDone++;
            controller.UpdateProgress(tasksDone, totalTasks, "Exporting Solution " + request.Solution.Name);

            var uniqueName = (string)solution.GetStringField(Fields.solution_.uniquename);
            var req = new ExportSolutionRequest();
            req.Managed = request.InstallAsManaged;
            req.SolutionName = uniqueName;
            var exportResponse = (ExportSolutionResponse)service.Execute(req);
            request.SetSolutionZipContent(exportResponse.ExportSolutionFile);

            tasksDone++;
            controller.UpdateProgress(tasksDone, totalTasks, "Exporting Solution " + request.Solution.Name);
            var targetXrmRecordService = new XrmRecordService(request.TargetConnection, ConnectionFactory);
            var importSolutionService = new ImportSolutionsService(targetXrmRecordService);
            var importResponse = importSolutionService.ImportSolutions(new SolutionsImport.ImportSolutionsRequest
            {
                 Items = new[] { request }
            }, controller);
            response.LoadImportSolutionsResponse(importResponse);
            response.ConnectionDeployedInto = request.TargetConnection;

            tasksDone++;
            if (solution.GetStringField(Fields.solution_.version) != request.SetSourceVersionPostRelease)
            {
                controller.UpdateProgress(tasksDone, totalTasks, "Setting New Solution Version " + request.SetSourceVersionPostRelease);
                solution.SetField(Fields.solution_.version, request.SetSourceVersionPostRelease);
                service.Update(solution, new[] { Fields.solution_.version });
            }

            response.Message = $"The solution has been deployed into {request.TargetConnection}";
        }
    }
}