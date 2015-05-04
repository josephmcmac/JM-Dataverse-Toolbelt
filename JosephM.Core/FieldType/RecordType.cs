using System.Runtime.Serialization;

namespace JosephM.Core.FieldType
{
    [DataContract]
    public class RecordType : PicklistOption
    {
        public RecordType()
        {
        }

        public RecordType(string key, string value) : base(key, value)
        {
        }
    }
}