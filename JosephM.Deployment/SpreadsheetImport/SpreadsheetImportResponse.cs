using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public void LoadParseResponse(ParseIntoEntitiesResponse parseResponse)
        {
            AddResponseItems(parseResponse.ResponseItems.Select(ri => new DataImportResponseItem(ri.TargetType,
                ri.TargetField,
                ri.Name,
                ri.StringValue,
                ri.Message,
                ri.Exception)));
        }
    }
}