using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Service;
using JosephM.Record.Attributes;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using JosephM.Record.Query;

namespace JosephM.Application.Desktop.Module.Crud.ConfigureAutonumber
{
    public class ConfigureAutonumberRequest : ServiceRequestBase
    {
        public ConfigureAutonumberRequest(RecordType recordType)
        {
            RecordType = recordType;
        }

        public ConfigureAutonumberRequest()
        {

        }

        [ReadOnlyWhenSet]
        [RecordTypeFor((nameof(Field)))]
        public RecordType RecordType { get; set; }

        [ReadOnlyWhenSet]
        [LookupCondition(nameof(IFieldMetadata.FieldType), ConditionType.Equal, RecordFieldType.String)]
        public RecordField Field { get; set; }
        public string AutonumberFormat { get; set; }

        [DisplayName("Set Seed (Optional)")]
        public long? SetSeed { get; set; }
    }
}