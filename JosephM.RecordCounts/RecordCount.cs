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

        public string RecordType { get; set; }
        public long Count { get; set; }
    }
}
