using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.MigrateRecords
{
    public class MigrateRecordsResponse : ServiceResponseBase<DataImportResponseItem>
    {
        private List<ImportedRecords> _importedRecords = new List<ImportedRecords>();
        public MigrateRecordsResponse()
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

        [PropertyInContextByPropertyValue(nameof(IsImportSummary), true)]
        public IEnumerable<ImportedRecords> ImportSummary
        {
            get
            {
                return _importedRecords;
            }
        }
    }
}