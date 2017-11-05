
using JosephM.Application.ViewModel.Attributes;
using JosephM.Application.ViewModel.SettingTypes;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using System.Collections.Generic;

namespace JosephM.Deployment.ExportXml
{
    [BulkAddRecordTypeFunction]
    [Group(Sections.Main, true, 10)]
    [Group(Sections.Fields, true, 15)]
    [Group(Sections.Fetch, false, 20)]
    public class ExportRecordType
    {
        public ExportRecordType()
        {
            IncludeAllFields = true;
        }

        [DisplayOrder(0)]
        [Group(Sections.Main)]
        [RequiredProperty]
        public ExportType Type { get; set; }

        [DisplayOrder(10)]
        [PropertyInContextByPropertyNotNull(nameof(Type))]
        [Group(Sections.Main)]
        [RequiredProperty]
        [ReadOnlyWhenSet]
        [RecordTypeFor(nameof(IncludeOnlyTheseFieldsInExportedRecords) + "." + nameof(RecordField))]
        [RecordTypeFor(nameof(SpecificRecordsToExport) + "." + nameof(LookupSetting.Record))]
        public RecordType RecordType { get; set; }

        [DisplayOrder(20)]
        [PropertyInContextByPropertyNotNull(nameof(Type))]
        [Group(Sections.Main)]
        public bool IncludeInactiveRecords { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(Type), ExportType.SpecificRecords)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        public IEnumerable<LookupSetting> SpecificRecordsToExport { get; set; }

        [Group(Sections.Fields)]
        [PropertyInContextByPropertyNotNull(nameof(RecordType))]
        public bool IncludeAllFields { get; set; }

        [Group(Sections.Fields)]
        [PropertyInContextByPropertyValue(nameof(IncludeAllFields), false)]
        [RequiredProperty]
        public IEnumerable<FieldSetting> IncludeOnlyTheseFieldsInExportedRecords { get; set; }

        [DisplayOrder(100)]
        [Group(Sections.Fetch)]
        [DisplayName("Fetch XML")]
        [Multiline]
        [RequiredProperty]
        [PropertyInContextByPropertyValue(nameof(Type), ExportType.FetchXml)]
        public string FetchXml { get; set; }

        public override string ToString()
        {
            return RecordType == null ? "Null" : RecordType.Value;
        }

        private static class Sections
        {
            public const string Main = "Export Type Details";
            public const string Fields = "Fields To Include";
            public const string Fetch = "Fetch XML - Note Attributes in The Entered XML Will Be Ignored. All fields Will Be Included Apart From Those Selected For Exclusion";
        }
    }
}
