using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Deployment.SolutionsImport;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.DataImportExport.XmlExport;
using JosephM.XrmModule.SavedXrmConnections;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.DeployPackage
{
    [Group(Sections.Summary, false, 0)]
    public class DeployPackageResponse : ServiceResponseBase<DataImportResponseItem>
    {
        private List<ImportedRecords> _importedRecords = new List<ImportedRecords>();

        public void LoadImportXmlResponse(ImportXmlResponse dataImportResponse)
        {
            if (dataImportResponse.Exception != null)
                AddResponseItem(new DataImportResponseItem("Fatal data import error", dataImportResponse.Exception));
            AddResponseItems(dataImportResponse.ResponseItems);
            _importedRecords.AddRange(dataImportResponse.ImportSummary);
        }

        public void LoadImportSolutionsResponse(ImportSolutionsResponse importSolutionsResponse)
        {
            AddResponseItems(importSolutionsResponse.ImportedSolutionResults.Select(it => new DataImportResponseItem(it.Type, null, it.Name, null, $"{it.Result} - {it.ErrorCode} - {it.ErrorText}", null, it.GetUrl())));
            FailedSolution = importSolutionsResponse.FailedSolution;
            FailedSolutionXml = importSolutionsResponse.FailedSolutionXml;
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

        [AllowGridFullScreen]
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

        [Hidden]
        public SavedXrmRecordConfiguration Connection { get; internal set; }

        private static class Sections
        {
            public const string Summary = "Summary";
        }
    }
}