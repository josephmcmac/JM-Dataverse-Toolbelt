using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Deployment.SolutionsImport;
using JosephM.XrmModule.SavedXrmConnections;
using System.Linq;

namespace JosephM.Deployment.SolutionTransfer
{
    [Group(Sections.Summary, false, 0)]
    public class SolutionTransferResponse : ServiceResponseBase<SolutionTransferResponseItem>
    {
        [Hidden]
        public SavedXrmRecordConfiguration ConnectionDeployedInto { get; set; }

        [DisplayOrder(10)]
        [Group(Sections.Summary)]
        [PropertyInContextByPropertyNotNull(nameof(FailedSolution))]
        public string FailedSolution { get; private set; }
        [Group(Sections.Summary)]
        [DisplayOrder(20)]
        [PropertyInContextByPropertyNotNull(nameof(FailedSolutionXml))]
        public string FailedSolutionXml { get; private set; }

        public void LoadImportSolutionsResponse(SolutionsImportResponse importSolutionResponse)
        {
            AddResponseItems(importSolutionResponse.ImportedSolutionResults.Select(i => new SolutionTransferResponseItem(i)).ToArray());
            FailedSolution = importSolutionResponse.FailedSolution;
            FailedSolutionXml = importSolutionResponse.FailedSolutionXml;
        }

        private static class Sections
        {
            public const string Summary = "Summary";
        }
    }
}