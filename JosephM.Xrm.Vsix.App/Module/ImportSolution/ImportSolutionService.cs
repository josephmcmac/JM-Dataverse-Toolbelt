using JosephM.Core.Log;
using JosephM.Core.Service;
using JosephM.Deployment.DeployPackage;
using JosephM.Record.Xrm.XrmRecord;

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
            service.ImportSolutions(new[] { request.SolutionZip.FileName }, controller, xrmRecordService.XrmService);
        }
    }
}