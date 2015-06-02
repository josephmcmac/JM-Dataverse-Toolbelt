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
    }
}
