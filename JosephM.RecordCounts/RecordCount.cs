using JosephM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
