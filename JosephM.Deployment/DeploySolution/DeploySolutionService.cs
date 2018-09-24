#region

using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.DataImport;
using JosephM.Deployment.ImportXml;
using JosephM.Deployment.SolutionImport;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;
using System.Xml;

#endregion

namespace JosephM.Deployment.DeploySolution
{
    public class DeploySolutionService :
        ServiceBase<DeploySolutionRequest, DeploySolutionResponse, DataImportResponseItem>
    {
        public DeploySolutionService()
        {
        }

        public override void ExecuteExtention(DeploySolutionRequest request, DeploySolutionResponse response,
            LogController controller)
        {
            DeploySolution(request, controller, response);
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
            importSolutionService.ImportSolutions(new Dictionary<string, byte[]>
            {
                { uniqueName, exportResponse.ExportSolutionFile }
            }, controller);

            tasksDone++;
            if (solution.GetStringField(Fields.solution_.version) != request.SetVersionPostRelease)
            {
                controller.UpdateProgress(tasksDone, totalTasks, "Setting New Solution Version " + request.SetVersionPostRelease);
                solution.SetField(Fields.solution_.version, request.SetVersionPostRelease);
                service.Update(solution, new[] { Fields.solution_.version });
            }
        }
    }
}