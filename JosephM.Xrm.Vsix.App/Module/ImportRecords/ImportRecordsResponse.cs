using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.DataImportExport.XmlExport;
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

        public void LoadDataImport(ImportXmlResponse dataImportResponse)
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