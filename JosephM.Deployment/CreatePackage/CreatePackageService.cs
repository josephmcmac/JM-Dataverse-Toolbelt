using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.DataImport;
using JosephM.Deployment.DeployPackage;
using JosephM.Deployment.ExportXml;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.Schema;
using Microsoft.Crm.Sdk.Messages;
using System;
using System.IO;
using System.Linq;

namespace JosephM.Deployment.CreatePackage
{
    public class CreatePackageService :
        ServiceBase<CreatePackageRequest, CreatePackageResponse, DataImportResponseItem>
    {
        public CreatePackageService(XrmRecordService xrmRecordService, IOrganizationConnectionFactory organizationConnectionFactory)
        {
            XrmRecordService = xrmRecordService;
            OrganizationConnectionFactory = organizationConnectionFactory;
        }

        public XrmRecordService XrmRecordService { get; }
        public IOrganizationConnectionFactory OrganizationConnectionFactory { get; }

        public override void ExecuteExtention(CreatePackageRequest request, CreatePackageResponse response,
            ServiceRequestController controller)
        {
            CreateDeploymentPackage(request, controller, response);
        }


        private void CreateDeploymentPackage(CreatePackageRequest request, ServiceRequestController controller, CreatePackageResponse response)
        {
            var folderPath = request.FolderPath.FolderPath;

            var tasksDone = 0;
            var totalTasks = 4;

            var xrmRecordService = XrmRecordService;
            var service = xrmRecordService.XrmService;
            var solution = service.Retrieve(Entities.solution, new Guid(request.Solution.Id));
            tasksDone++;
            var solutionUniqueName = solution.GetStringField(Fields.solution_.uniquename);
            if (solution.GetStringField(Fields.solution_.version) != request.ThisReleaseVersion)
            {
                controller.UpdateProgress(tasksDone, totalTasks, "Setting Release Version " + request.ThisReleaseVersion);
                solution.SetField(Fields.solution_.version, request.ThisReleaseVersion);
                service.Update(solution, new[] { Fields.solution_.version });
            }
            controller.UpdateProgress(tasksDone, totalTasks, "Exporting Solution " + solutionUniqueName);

            var req = new ExportSolutionRequest();
            req.Managed = request.ExportAsManaged;
            req.SolutionName = solutionUniqueName;

            var exportResponse = (ExportSolutionResponse)service.Execute(req);

            var version = solution.GetStringField(Fields.solution_.version);
            var versionText = version == null ? null : version.Replace(".", "_");
            var fileName = string.Format("{0}_{1}{2}.zip", solutionUniqueName, versionText,
                request.ExportAsManaged ? "_managed" : null);

            FileUtility.WriteToFile(folderPath, fileName, exportResponse.ExportSolutionFile);
            ++tasksDone;
            if (request.DataToInclude != null && request.DataToInclude.Any())
            {
                controller.UpdateProgress(tasksDone, totalTasks, "Exporting Data");
                var dataExportService = new ExportXmlService(xrmRecordService);
                dataExportService.ExportXml(request.DataToInclude,
                    new Folder(GetDataExportFolder(folderPath)), request.IncludeNotes, request.IncludeFileAndImageFields, request.IncludeNNRelationshipsBetweenEntities, controller.Controller);
            }
            tasksDone++;
            if (solution.GetStringField(Fields.solution_.version) != request.SetVersionPostRelease)
            {
                controller.UpdateProgress(tasksDone, totalTasks, "Setting New Solution Version " + request.SetVersionPostRelease);
                solution.SetField(Fields.solution_.version, request.SetVersionPostRelease);
                service.Update(solution, new[] { Fields.solution_.version });
            }
            if (request.DeployPackageInto != null)
            {
                if (response.HasError)
                {
                    throw new Exception("Package Deployment Aborted Due To Errors During Creating");
                }
                else
                {
                    var deployRequest = new DeployPackageRequest
                    {
                        FolderContainingPackage = request.FolderPath,
                        Connection = request.DeployPackageInto
                    };
                    var deployService = new DeployPackageService(OrganizationConnectionFactory);
                    var deployPackageResponse = new DeployPackageResponse();
                    deployService.ExecuteExtention(deployRequest, deployPackageResponse, controller);
                    response.LoadDeployPackageResponse(deployPackageResponse);
                }
            }

            response.Message = "The Deployment Package Has Been Generated";


            if (request.DeployPackageInto != null)
            {
                if (response.FailedSolution != null)
                {
                    response.Message += " but there was an error deploying the solution";
                }
                else
                {
                    response.Message += " and Deployed";
                }
            }
        }

        private string GetDataExportFolder(string rootFolder)
        {
            return rootFolder == null ? null : Path.Combine(rootFolder, "Data");
        }
    }
}