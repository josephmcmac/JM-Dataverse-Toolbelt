using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;

namespace JosephM.Deployment.ImportXml
{
    public class ImportXmlResponse : ServiceResponseBase<DataImportResponseItem>
    {
        private List<ImportedRecords> _importedRecords = new List<ImportedRecords>();

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