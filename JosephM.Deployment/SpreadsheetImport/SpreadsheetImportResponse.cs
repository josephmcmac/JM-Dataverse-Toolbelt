using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace JosephM.Deployment.SpreadsheetImport
{
    public class SpreadsheetImportResponse : ServiceResponseBase<DataImportResponseItem>
    {
        private List<ImportedRecords> _importedRecords = new List<ImportedRecords>();
        public SpreadsheetImportResponse()
        {
        }

        public IEnumerable<ImportedRecords> GetImportSummary()
        {
            return _importedRecords;
        }

        public void LoadDataImport(DataImportResponse dataImportResponse)
        {
            AddResponseItems(dataImportResponse.ResponseItems);
            _importedRecords.AddRange(dataImportResponse.GetImportSummary());
        }
    }
}