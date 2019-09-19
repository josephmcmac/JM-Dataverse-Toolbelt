using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using JosephM.Deployment.ImportXml;
using JosephM.Deployment.SolutionImport;
using JosephM.Record.Xrm.XrmRecord;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JosephM.Deployment.DeployPackage
{
    public class DeployPackageService :
        ServiceBase<DeployPackageRequest, DeployPackageResponse, DataImportResponseItem>
    {
        public DeployPackageService()
        {
        }

        public override void ExecuteExtention(DeployPackageRequest request, DeployPackageResponse response,
            ServiceRequestController controller)
        {
            DeployPackage(request, controller, response);
        }

        private void DeployPackage(DeployPackageRequest request, ServiceRequestController controller, DeployPackageResponse response)
        {
            var xrmRecordService = new XrmRecordService(request.Connection, controller.Controller);
            var packageFolder = request.FolderContainingPackage.FolderPath;
            var solutionFiles = Directory.GetFiles(packageFolder, "*.zip")
                .OrderBy(s => s)
                .ToArray();

            var importItems = ImportSolutions(solutionFiles, controller.Controller, xrmRecordService);
            response.Connection = request.Connection;
            response.AddResponseItems(importItems.Select(it => new DataImportResponseItem(it.Type, null, it.Name, null, $"{it.Result} - {it.ErrorCode} - {it.ErrorText}", null, it.GetUrl())));

            foreach (var childFolder in Directory.GetDirectories(packageFolder))
            {
                if (new DirectoryInfo(childFolder).Name == "Data")
                {
                    var dataImportService = new ImportXmlService(xrmRecordService);
                    var importResponse = new ImportXmlResponse();
                    dataImportService.ImportXml(request, controller, importResponse, executeMultipleSetSize: 10, targetCacheLimit: 200);
                    response.LoadImportxmlResponse(importResponse);
                }
            }

            response.Message = $"The Package Has Been Deployed Into {request.Connection}";
        }

        public IEnumerable<SolutionImportResult> ImportSolutions(IEnumerable<string> solutionFiles, LogController controller, XrmRecordService xrmRecordService)
        {
            var solutionFilesDictionary = new Dictionary<string, byte[]>();
            foreach(var item in solutionFiles)
            {
                solutionFilesDictionary.Add(new FileInfo(item).Name, File.ReadAllBytes(item));
            }
            var solutionImportService = new SolutionImportService(xrmRecordService);
            return solutionImportService.ImportSolutions(solutionFilesDictionary, controller);
        }
    }
}