using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Deployment.ImportXml;
using JosephM.XrmModule.SavedXrmConnections;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.DataImport
{
    [Group(Sections.Summary, false, 0)]
    public class DeployPackageResponse : ServiceResponseBase<DataImportResponseItem>
    {
        private List<ImportedRecords> _importedRecords = new List<ImportedRecords>();

        public void LoadImportxmlResponse(ImportXmlResponse dataImportResponse)
        {
            if (dataImportResponse.Exception != null)
                AddResponseItem(new DataImportResponseItem("Fatal Data Import Error", dataImportResponse.Exception));
            AddResponseItems(dataImportResponse.ResponseItems);
            _importedRecords.AddRange(dataImportResponse.ImportSummary);
        }

        [Hidden]
        public bool IsImportSummary
        {
            get { return ImportSummary != null && ImportSummary.Any(); }
        }

        [AllowGridFullScreen]
        [Group(Sections.Summary)]
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