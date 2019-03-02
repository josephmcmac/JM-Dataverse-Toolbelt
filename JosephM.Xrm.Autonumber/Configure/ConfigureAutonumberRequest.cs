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

        [DisplayOrder(10)]
        [ReadOnlyWhenSet]
        [RequiredProperty]
        [RecordTypeFor((nameof(Field)))]
        public RecordType RecordType { get; set; }

        [DisplayOrder(20)]
        [ReadOnlyWhenSet]
        [RequiredProperty]
        [LookupCondition(nameof(IFieldMetadata.FieldType), ConditionType.Equal, RecordFieldType.String)]
        public RecordField Field { get; set; }

        [DisplayOrder(30)]
        public string AutonumberFormat { get; set; }

        [DisplayOrder(40)]
        [DisplayName("Set Seed (Optional)")]
        public long? SetSeed { get; set; }
    }
}