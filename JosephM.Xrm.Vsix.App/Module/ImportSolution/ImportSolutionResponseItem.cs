using JosephM.Core.Service;
using JosephM.Deployment.DeployPackage;

namespace JosephM.Xrm.Vsix.Module.ImportSolution
{
    public class ImportSolutionResponseItem : ServiceResponseItem
    {
        public ImportSolutionResponseItem(SolutionImportResult solutionImportResult)
        {
            SolutionImportResult = solutionImportResult;
        }

        private SolutionImportResult SolutionImportResult { get; }
        public string Type => SolutionImportResult.Type;
        public string Name => SolutionImportResult.Name;
        public string Result => SolutionImportResult.Result;
        public string ErrorCode => SolutionImportResult.ErrorCode;
        public string ErrorText => SolutionImportResult.ErrorText;
    }
}