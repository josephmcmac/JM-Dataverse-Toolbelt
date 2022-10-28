using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Deployment.DeployPackage;
using JosephM.Xrm.DataImportExport.Import;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.CreatePackage
{
    [Group(Sections.Summary, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 0)]
    public class CreatePackageResponse : ServiceResponseBase<DataImportResponseItem>
    {
        private List<ImportedRecords> _importedRecords = new List<ImportedRecords>();

        public void LoadDeployPackageResponse(DeployPackageResponse deployPackageResponse)
        {
            if (deployPackageResponse.Exception != null)
                AddResponseItem(new DataImportResponseItem("Fatal deploy error", deployPackageResponse.Exception));
            AddResponseItems(deployPackageResponse.ResponseItems);
            _importedRecords.AddRange(deployPackageResponse.ImportSummary);
            FailedSolution = deployPackageResponse.FailedSolution;
            FailedSolutionXml = deployPackageResponse.FailedSolutionXml;
        }

        [DisplayOrder(10)]
        [Group(Sections.Summary)]
        [PropertyInContextByPropertyNotNull(nameof(FailedSolution))]
        public string FailedSolution { get; private set; }

        [Group(Sections.Summary)]
        [DisplayOrder(20)]
        [PropertyInContextByPropertyNotNull(nameof(FailedSolutionXml))]
        public string FailedSolutionXml { get; private set; }

        [Hidden]
        public bool IsImportSummary
        {
            get { return ImportSummary != null && ImportSummary.Any(); }
        }

        [Group(Sections.Summary)]
        [DisplayOrder(30)]
        [PropertyInContextByPropertyValue(nameof(IsImportSummary), true)]
        public IEnumerable<ImportedRecords> ImportSummary
        {
            get
            {
                return _importedRecords;
            }
        }

        private static class Sections
        {
            public const string Summary = "Summary";
        }
    }
}