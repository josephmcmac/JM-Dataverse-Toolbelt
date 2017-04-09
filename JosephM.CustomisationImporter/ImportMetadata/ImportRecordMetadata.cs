using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Record.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.CustomisationImporter.ImportMetadata
{
    [DisplayName("Record Type")]
    public class ImportRecordMetadata : RecordMetadata
    {
        public ImportRecordMetadata()
        {
        }

        public bool ViewUpdated { get; set; }
    }
}
