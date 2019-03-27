using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using JosephM.Deployment.SolutionImport;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.DeploySolution
{
    public class DeploySolutionService :
        ServiceBase<DeploySolutionRequest, DeploySolutionResponse, DeploySolutionResponseItem>
    {
        public DeploySolutionService()
        {
        }

        public override void ExecuteExtention(DeploySolutionRequest request, DeploySolutionResponse response,
            ServiceRequestController controller)
        {
            DeploySolution(request, controller.Controller, response);
        }

        private void DeploySolution(DeploySolutionRequest request, LogController controller, DeploySolutionResponse response)
        {
            var tasksDone = 0;
            var totalTasks = 4;

            var sourceXrmRecordService = new XrmRecordService(request.SourceConnection);
            var service = sourceXrmRecordService.XrmService;
            var solution = service.Retrieve(Entities.solution, new Guid(request.Solution.Id));
            tasksDone++;
            if (solution.GetStringField(Fields.solution_.version) != request.ThisReleaseVersion)
            {
                controller.UpdateProgress(tasksDone, totalTasks, "Setting Release Version " + request.ThisReleaseVersion);
                solution.SetField(Fields.solution_.version, request.ThisReleaseVersion);
                service.Update(solution, new[] { Fields.solution_.version });
            }
            tasksDone++;
            controller.UpdateProgress(tasksDone, totalTasks, "Exporting Solution " + request.Solution.Name);

            var uniqueName = (string)solution.GetStringField(Fields.solution_.uniquename);
            var req = new ExportSolutionRequest();
            req.Managed = request.ExportAsManaged;
            req.SolutionName = uniqueName;
            var exportResponse = (ExportSolutionResponse)service.Execute(req);

            tasksDone++;
            controller.UpdateProgress(tasksDone, totalTasks, "Exporting Solution " + request.Solution.Name);
            var targetXrmRecordService = new XrmRecordService(request.TargetConnection);
            var importSolutionService = new SolutionImportService(targetXrmRecordService);
            var importResponse = importSolutionService.ImportSolutions(new Dictionary<string, byte[]>
            {
                { uniqueName, exportResponse.ExportSolutionFile }
            }, controller);
            response.AddResponseItems(importResponse.Select(i => new DeploySolutionResponseItem(i)).ToArray());
            response.ConnectionDeployedInto = request.TargetConnection;

            tasksDone++;
            if (solution.GetStringField(Fields.solution_.version) != request.SetVersionPostRelease)
            {
                controller.UpdateProgress(tasksDone, totalTasks, "Setting New Solution Version " + request.SetVersionPostRelease);
                solution.SetField(Fields.solution_.version, request.SetVersionPostRelease);
                service.Update(solution, new[] { Fields.solution_.version });
            }

            response.Message = $"The Solution Has Been Deployed Into {request.TargetConnection}";
        }
    }
}