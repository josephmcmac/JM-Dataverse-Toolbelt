using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Record.Application.SettingTypes;

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
        public IEnumerable<LookupSetting> OnlyExportSpecificRecords { get; set; }

        //todo no validation of fetch query in ui
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
