using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Core.Utility;
using JosephM.Deployment.DeployPackage;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.DataImportExport.XmlImport;
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

            var xrmRecordService = XrmRecordService;
            var service = xrmRecordService.XrmService;
            var solution = service.Retrieve(Entities.solution, new Guid(request.Solution.Id));
            var solutionUniqueName = solution.GetStringField(Fields.solution_.uniquename);

            if (solution.GetStringField(Fields.solution_.version) != request.ThisReleaseVersion)
            {
                controller.LogLiteral("Setting Release Version " + request.ThisReleaseVersion);
                solution.SetField(Fields.solution_.version, request.ThisReleaseVersion);
                service.Update(solution, new[] { Fields.solution_.version });
            }
            controller.LogLiteral("Exporting Solution " + solutionUniqueName);

            var req = new ExportSolutionRequest();
            req.Managed = request.ExportAsManaged;
            req.SolutionName = solutionUniqueName;

            var exportResponse = (ExportSolutionResponse)service.Execute(req);

            var version = solution.GetStringField(Fields.solution_.version);
            var versionText = version == null ? null : version.Replace(".", "_");
            var fileName = string.Format("{0}_{1}{2}.zip", solutionUniqueName, versionText,
                request.ExportAsManaged ? "_managed" : null);

            FileUtility.WriteToFile(folderPath, fileName, exportResponse.ExportSolutionFile);
            if (request.DataToInclude != null && request.DataToInclude.Any())
            {
                controller.LogLiteral("Setting Release Version " + request.ThisReleaseVersion);
                var dataExportService = new ExportXmlService(xrmRecordService);
                dataExportService.ExportXml(request.DataToInclude,
                    new Folder(GetDataExportFolder(folderPath)), request.IncludeNotes, request.IncludeFileAndImageFields, request.IncludeNNRelationshipsBetweenEntities, controller.Controller);
            }
            if (solution.GetStringField(Fields.solution_.version) != request.SetVersionPostRelease)
            {
                controller.LogLiteral("Setting New Solution Version " + request.SetVersionPostRelease);
                solution.SetField(Fields.solution_.version, request.SetVersionPostRelease);
                service.Update(solution, new[] { Fields.solution_.version });
            }
            response.Message = "The Deployment Package Has Been Generated";
        }

        private string GetDataExportFolder(string rootFolder)
        {
            return rootFolder == null ? null : Path.Combine(rootFolder, "Data");
        }
    }
}