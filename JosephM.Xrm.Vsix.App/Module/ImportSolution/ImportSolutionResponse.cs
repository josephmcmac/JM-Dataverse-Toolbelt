using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Deployment.SolutionImport;
using JosephM.XrmModule.SavedXrmConnections;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.ImportSolution
{
    [Group(Sections.Summary, false, 0)]
    public class ImportSolutionResponse : ServiceResponseBase<ImportSolutionResponseItem>
    {
        [Hidden]
        public SavedXrmRecordConfiguration Connection { get; internal set; }

        [DisplayOrder(10)]
        [Group(Sections.Summary)]
        [PropertyInContextByPropertyNotNull(nameof(FailedSolution))]
        public string FailedSolution { get; private set; }
        [Group(Sections.Summary)]
        [DisplayOrder(20)]
        [PropertyInContextByPropertyNotNull(nameof(FailedSolutionXml))]
        public string FailedSolutionXml { get; private set; }

        public void LoadImportSolutionsResponse(ImportSolutionsResponse importSolutionsResponse)
        {
            AddResponseItems(importSolutionsResponse.ImportedSolutionResults.Select(si => new ImportSolutionResponseItem(si)));
            FailedSolution = importSolutionsResponse.FailedSolution;
            FailedSolutionXml = importSolutionsResponse.FailedSolutionXml;
        }

        private static class Sections
        {
            public const string Summary = "Summary";
        }
    }
}