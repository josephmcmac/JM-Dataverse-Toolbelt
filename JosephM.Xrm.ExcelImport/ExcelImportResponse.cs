using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Record.Xrm.XrmRecord;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.Xrm.DataImportExport.MappedImport;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.ExcelImport
{
    [Group(Sections.Summary, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 0)]
    public class ExcelImportResponse : ServiceResponseBase<ExcelImportResponseItem>
    {
        private List<ImportedRecords> _importedRecords = new List<ImportedRecords>();

        public void LoadSpreadsheetImport(MappedImportResponse dataImportResponse)
        {
            AddResponseItems(dataImportResponse.ResponseItems.Select(r => new ExcelImportResponseItem(r)));
            _importedRecords.AddRange(dataImportResponse.GetImportSummary());
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
        public IXrmRecordConfiguration Connection { get; internal set; }

        private static class Sections
        {
            public const string Summary = "Summary";
        }
    }
}