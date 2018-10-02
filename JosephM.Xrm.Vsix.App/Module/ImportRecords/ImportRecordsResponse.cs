using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.Vsix.Module.ImportRecords
{
    [Group(Sections.Summary, false, 0)]
    public class ImportRecordsResponse : ServiceResponseBase<DataImportResponseItem>
    {
        private List<ImportedRecords> _importedRecords = new List<ImportedRecords>();
        public ImportRecordsResponse()
        {
        }

        public void LoadDataImport(DataImportResponse dataImportResponse)
        {
            AddResponseItems(dataImportResponse.ResponseItems);
            _importedRecords.AddRange(dataImportResponse.GetImportSummary());
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