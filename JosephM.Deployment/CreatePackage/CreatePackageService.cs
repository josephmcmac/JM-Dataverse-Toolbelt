#region

using JosephM.Core.Extentions;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.DeployPackage;
using JosephM.Deployment.ExportXml;
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

#endregion

namespace JosephM.Deployment.CreatePackage
{
    public class CreatePackageService :
        DataImportServiceBase<CreatePackageRequest, ServiceResponseBase<DataImportResponseItem>, DataImportResponseItem>
    {
        public CreatePackageService(XrmRecordService xrmRecordService)
            : base(xrmRecordService)
        {
        }

        public override void ExecuteExtention(CreatePackageRequest request, ServiceResponseBase<DataImportResponseItem> response,
            LogController controller)
        {
            //todo catch fatal error in response in base class
            CreateDeploymentPackage(request, controller, response);
        }


        private void CreateDeploymentPackage(CreatePackageRequest request, LogController controller, ServiceResponseBase<DataImportResponseItem> response)
        {
            var folderPath = request.FolderPath.FolderPath;

            var tasksDone = 0;
            var totalTasks = 3;

            var xrmRecordService = XrmRecordService;
            var service = xrmRecordService.XrmService;
            var solution = service.Retrieve(Entities.solution, new Guid(request.Solution.Id));
            tasksDone++;
            if (solution.GetStringField(Fields.solution_.version) != request.ThisReleaseVersion)
            {
                controller.UpdateProgress(tasksDone, totalTasks, "Setting Release Version " + request.ThisReleaseVersion);
                solution.SetField(Fields.solution_.version, request.ThisReleaseVersion);
                service.Update(solution, new[] { Fields.solution_.version });
            }
            controller.UpdateProgress(tasksDone, totalTasks, "Exporting Solution " + request.Solution.Name);

            var uniqueName = (string)solution.GetStringField(Fields.solution_.uniquename);
            var req = new ExportSolutionRequest();
            req.Managed = request.ExportAsManaged;
            req.SolutionName = uniqueName;

            var exportResponse = (ExportSolutionResponse)service.Execute(req);

            var version = solution.GetStringField(Fields.solution_.version);
            var versionText = version == null ? null : version.Replace(".", "_");
            var fileName = string.Format("{0}_{1}{2}.zip", uniqueName, versionText,
                request.ExportAsManaged ? "_managed" : null);

            FileUtility.WriteToFile(folderPath, fileName, exportResponse.ExportSolutionFile);
            ++tasksDone;
            if (request.DataToInclude != null && request.DataToInclude.Any())
            {
                controller.UpdateProgress(tasksDone, totalTasks, "Exporting Data " + request.Solution.Name);
                var dataExportService = new ExportXmlService(xrmRecordService);
                dataExportService.ExportXml(request.DataToInclude,
                    new Folder(GetDataExportFolder(folderPath)), request.IncludeNotes,
                    request.IncludeNNRelationshipsBetweenEntities, controller);
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
                    var deployService = new DeployPackageService(new XrmRecordService(request.DeployPackageInto));
                    deployService.ExecuteExtention(deployRequest, response, controller);
                }
            }
        }

        private string GetDataExportFolder(string rootFolder)
        {
            return rootFolder == null ? null : Path.Combine(rootFolder, "Data");
        }
    }
}