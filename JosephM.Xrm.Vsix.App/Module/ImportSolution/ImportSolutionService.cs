using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.DeployPackage;
using JosephM.Record.Xrm.XrmRecord;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.ImportSolution
{
    public class ImportSolutionService :
        ServiceBase<ImportSolutionRequest, ImportSolutionResponse, ImportSolutionResponseItem>
    {
        public override void ExecuteExtention(ImportSolutionRequest request, ImportSolutionResponse response,
            LogController controller)
        {
            //just use the method in DeployPackageService to do the import
            var xrmRecordService = new XrmRecordService(request.Connection);
            var service = new DeployPackageService(xrmRecordService);
            var importItems = service.ImportSolutions(new[] { request.SolutionZip.FileName }, controller, xrmRecordService);
            response.AddResponseItems(importItems.Select(it => new ImportSolutionResponseItem(it)));
        }
    }
}