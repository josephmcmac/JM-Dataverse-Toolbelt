
using System.Collections.Generic;
using JosephM.Application.ViewModel.SettingTypes;
using System.Configuration;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.Xrm.ImportExporter.Service
{
    public class ImportExportRecordType
    {
        [RequiredProperty]
        public ExportType Type { get; set; }

        [RequiredProperty]
        [ReadOnlyWhenSet]
        [RecordTypeFor("ExcludeFields.RecordField")]
        [RecordTypeFor("OnlyExportSpecificRecords.Record")]
        public RecordType RecordType { get; set; }

        public bool IncludeInactive { get; set; }

        [PropertyInContextByPropertyNotNull("RecordType")]
        public IEnumerable<FieldSetting> ExcludeFields { get; set; }

        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", ExportType.SpecificRecords)]
        [PropertyInContextByPropertyNotNull("RecordType")]
        public IEnumerable<LookupSetting> OnlyExportSpecificRecords { get; set; }

        [DisplayName("Fetch XML (Attributes Ignored)")]
        [Multiline]
        [RequiredProperty]
        [PropertyInContextByPropertyValue("Type", ExportType.FetchXml)]
        public string FetchXml { get; set; }

        public override string ToString()
        {
            return RecordType == null ? "Null" : RecordType.Value;
        }
    }
}
