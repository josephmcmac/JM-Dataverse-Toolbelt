using JosephM.Core.Attributes;

namespace JosephM.RecordCounts
{
    public class RecordCount
    {
        public RecordCount(string recordType, long count)
        {
            RecordType = recordType;
            Count = count;
        }

        [DisplayOrder(20)]
        public string RecordType { get; set; }
        [DisplayOrder(30)]
        [GridWidth(125)]
        public long Count { get; set; }
    }
}
