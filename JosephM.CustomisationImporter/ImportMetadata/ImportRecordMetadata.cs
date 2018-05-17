using JosephM.Core.Attributes;
using JosephM.Record.Metadata;

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
