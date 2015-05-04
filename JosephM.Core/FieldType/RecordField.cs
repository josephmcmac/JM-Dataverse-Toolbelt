using System.Runtime.Serialization;

namespace JosephM.Core.FieldType
{
    [DataContract]
    public class RecordField : PicklistOption
    {
        public RecordField()
        {
        }

        public RecordField(string key, string value)
            : base(key, value)
        {
        }
    }
}