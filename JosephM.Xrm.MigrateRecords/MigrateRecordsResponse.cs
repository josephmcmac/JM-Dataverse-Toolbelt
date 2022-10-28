﻿using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Xrm.DataImportExport.Import;
using JosephM.XrmModule.SavedXrmConnections;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Xrm.MigrateRecords
{
    [Group(Sections.Summary, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 0)]
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
        public SavedXrmRecordConfiguration ConnectionMigratedInto { get; internal set; }

        private static class Sections
        {
            public const string Summary = "Summary";
        }
    }
}