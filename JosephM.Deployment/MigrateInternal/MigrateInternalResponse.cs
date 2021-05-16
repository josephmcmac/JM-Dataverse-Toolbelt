using JosephM.Application.Desktop.Module.Crud.BulkCopyFieldValue;
using JosephM.Core.Attributes;
using JosephM.Core.Service;
using JosephM.Deployment.DataImport;
using JosephM.Deployment.SpreadsheetImport;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Deployment.MigrateInternal
{
    [Group(Sections.Summary, false, 0)]
    public class MigrateInternalResponse : ServiceResponseBase<MigrateInternalResponseItem>
    {
        private List<ImportedRecords> _importedRecords = new List<ImportedRecords>();
        private List<MigratedLookupField> _migratedLookupFields = new List<MigratedLookupField>();

        public void LoadSpreadsheetImport(SourceImportResponse dataImportResponse)
        {
            AddResponseItems(dataImportResponse.ResponseItems.Select(r => new MigrateInternalResponseItem(r)));
            _importedRecords.AddRange(dataImportResponse.GetImportSummary());
        }

        [Hidden]
        public bool IsImportSummary
        {
            get { return ImportSummary != null && ImportSummary.Any(); }
        }

        [AllowDownload]
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
        public bool IsMigratedLookupFields
        {
            get { return MigratedLookupFields != null && MigratedLookupFields.Any(); }
        }

        [AllowDownload]
        [AllowGridFullScreen]
        [Group(Sections.Summary)]
        [PropertyInContextByPropertyValue(nameof(IsMigratedLookupFields), true)]
        public IEnumerable<MigratedLookupField> MigratedLookupFields
        {
            get
            {
                return _migratedLookupFields;
            }
        }

        private static class Sections
        {
            public const string Summary = "Summary";
        }

        public void LoadBulkCopy(MigratedLookupField migratedLookupField)
        {
            AddResponseItems(migratedLookupField.GetInternalResponse().ResponseItems.Select(r => new MigrateInternalResponseItem(migratedLookupField.EntityType, migratedLookupField.SourceField, r)));
            _migratedLookupFields.Add(migratedLookupField);
        }
    }
}