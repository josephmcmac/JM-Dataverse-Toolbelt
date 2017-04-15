
using System.Collections.Generic;
using JosephM.Application.ViewModel.SettingTypes;
using System.Configuration;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.Xrm.ImportExporter.Service
{
    [Group(Sections.Main, true)]
    [Group(Sections.Fetch, false)]
    public class ImportExportRecordType
    {
        [Group(Sections.Main)]
        [RequiredProperty]
        public ExportType Type { get; set; }

        [PropertyInContextByPropertyNotNull("Type")]
        [Group(Sections.Main)]
        [RequiredProperty]
        [ReadOnlyWhenSet]
        [RecordTypeFor("ExcludeTheseFieldsInExportedRecords.RecordField")]
        [RecordTypeFor("SpecificRecordsToExport.Record")]
        public RecordType RecordType { get; set; }

        [PropertyInContextByPropertyNotNull("Type")]
        [Group(Sections.Main)]
        public bool IncludeInactiveRecords { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", ExportType.SpecificRecords)]
        [PropertyInContextByPropertyNotNull("RecordType")]
        public IEnumerable<LookupSetting> SpecificRecordsToExport { get; set; }

        [PropertyInContextByPropertyNotNull("RecordType")]
        public IEnumerable<FieldSetting> ExcludeTheseFieldsInExportedRecords { get; set; }

        [Group(Sections.Fetch)]
        [DisplayName("Fetch XML")]
        [Multiline]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", ExportType.FetchXml)]
        public string FetchXml { get; set; }

        public override string ToString()
        {
            return RecordType == null ? "Null" : RecordType.Value;
        }

        private static class Sections
        {
            public const string Main = "Export Type Details";
            public const string Fetch = "Fetch XML - Note Attributes in The Entered XML Will Be Ignored. All fields Will Be Included Apart From Those Selected For Exclusion";
        }
    }
}
