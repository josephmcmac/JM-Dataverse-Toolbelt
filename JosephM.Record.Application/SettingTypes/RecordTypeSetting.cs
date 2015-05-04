using JosephM.Core.Attributes;
using JosephM.Core.FieldType;

namespace JosephM.Record.Application.SettingTypes
{
    public class RecordTypeSetting
    {
        public RecordTypeSetting()
        {
            
        }

        public RecordTypeSetting(string schemaName, string label)
        {
            RecordType = new RecordType(schemaName, label);
        }

        [RequiredProperty]
        public RecordType RecordType { get; set; }
    }
}