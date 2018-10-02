using JosephM.Core.Attributes;
using JosephM.Core.Service;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.DataImport
{
    [Group(Sections.Summary, false, 0)]
    public class CreatePackageResponse : ServiceResponseBase<DataImportResponseItem>
    {
        private List<ImportedRecords> _importedRecords = new List<ImportedRecords>();

        public void LoadDeployPackageResponse(DeployPackageResponse deployPackageResponse)
        {
            if (deployPackageResponse.Exception != null)
                AddResponseItem(new DataImportResponseItem("Fatal Deploy Error", deployPackageResponse.Exception));
            AddResponseItems(deployPackageResponse.ResponseItems);
            _importedRecords.AddRange(deployPackageResponse.ImportSummary);
        }

        [Hidden]
        public bool IsImportSummary
        {
            get { return ImportSummary != null && ImportSummary.Any(); }
        }

        [Group(Sections.Summary)]
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