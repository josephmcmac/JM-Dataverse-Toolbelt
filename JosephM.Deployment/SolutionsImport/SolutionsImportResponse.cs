using System.Collections.Generic;

namespace JosephM.Deployment.SolutionsImport
{
    public class SolutionsImportResponse
    {
        public bool Success { get { return FailedSolution == null; } }
        public string FailedSolution { get; set; }
        public string FailedSolutionXml { get; set; }

        private List<string> _importedSolutions = new List<string>();
        public IEnumerable<string> ImportedSolutions { get { return _importedSolutions; } }

        private List<SolutionImportResult> _importedSolutionResults = new List<SolutionImportResult>();
        public IEnumerable<SolutionImportResult> ImportedSolutionResults { get { return _importedSolutionResults; } }

        public void AddImported(string solution)
        {
            _importedSolutions.Add(solution);
        }

        public void AddResult(SolutionImportResult result)
        {
            _importedSolutionResults.Add(result);
        }
    }
}
