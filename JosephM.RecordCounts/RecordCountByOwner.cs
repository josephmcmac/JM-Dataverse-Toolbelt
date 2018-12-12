using JosephM.Core.Attributes;

namespace JosephM.RecordCounts
{
    public class RecordCountByOwner : RecordCount
    {
        public RecordCountByOwner(string recordType, long count, string ownerType, string owner)
            : base(recordType, count)
        {
            OwnerType = ownerType;
            Owner = owner;
        }

        [Hidden]
        public string OwnerType { get; set; }
        [DisplayOrder(10)]
        public string Owner { get; set; }
    }
}
