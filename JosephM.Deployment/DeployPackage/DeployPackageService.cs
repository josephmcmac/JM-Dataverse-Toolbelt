using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.SolutionsImport;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.DataImportExport.XmlExport;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Deployment.DeployPackage
{
    public class DeployPackageService :
        ServiceBase<DeployPackageRequest, DeployPackageResponse, DataImportResponseItem>
    {
        private IOrganizationConnectionFactory XrmServiceFactory { get; }

        public DeployPackageService(IOrganizationConnectionFactory xrmServiceFactory)
        {
            XrmServiceFactory = xrmServiceFactory;
        }

        public override void ExecuteExtention(DeployPackageRequest request, DeployPackageResponse response,
            ServiceRequestController controller)
        {
            DeployPackage(request, controller, response);
        }

        private void DeployPackage(DeployPackageRequest request, ServiceRequestController controller, DeployPackageResponse response)
        {
            var xrmRecordService = new XrmRecordService(request.Connection, controller.Controller, XrmServiceFactory);
            var packageFolder = request.FolderContainingPackage.FolderPath;
            var solutionFiles = Directory.GetFiles(packageFolder, "*.zip")
                .OrderBy(s => s)
                .ToArray();

            var solutionImportService = new ImportSolutionsService(xrmRecordService);
            var importSolutionResponse = solutionImportService.ImportSolutions(new ImportSolutionsRequest
            {
                Items = request.SolutionsForDeployment
            }, controller.Controller);
            response.Connection = request.Connection;
            response.LoadImportSolutionsResponse(importSolutionResponse);

            if (!importSolutionResponse.Success)
            {
                response.Message = $"There was an error importing the solution during deployment";
            }
            else
            {
                foreach (var childFolder in Directory.GetDirectories(packageFolder))
                {
                    if (new DirectoryInfo(childFolder).Name == "Data")
                    {
                        var dataImportService = new ImportXmlService(xrmRecordService);
                        var importResponse = new ImportXmlResponse();
                        dataImportService.ImportXml(request, controller, importResponse, executeMultipleSetSize: 10, targetCacheLimit: 200);
                        response.LoadImportXmlResponse(importResponse);
                    }
                }
                response.Message = $"The Package Has Been Deployed Into {request.Connection}";
            }
        }
    }
}