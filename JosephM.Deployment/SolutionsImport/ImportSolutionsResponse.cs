using System.Collections.Generic;

namespace JosephM.Deployment.SolutionsImport
{
    public class ImportSolutionsResponse
    {
        public bool Success { get { return FailedSolution == null; } }
        public string FailedSolution { get; set; }
        public string FailedSolutionXml { get; set; }

        private List<string> _importedSolutions = new List<string>();
        public IEnumerable<string> ImportedSolutions { get { return _importedSolutions; } }

        private List<ImportSolutionResult> _importedSolutionResults = new List<ImportSolutionResult>();
        public IEnumerable<ImportSolutionResult> ImportedSolutionResults { get { return _importedSolutionResults; } }

        public void AddImported(string solution)
        {
            _importedSolutions.Add(solution);
        }

        public void AddResult(ImportSolutionResult result)
        {
            _importedSolutionResults.Add(result);
        }
    }
}
