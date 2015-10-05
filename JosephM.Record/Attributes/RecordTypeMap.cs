using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Record.Attributes
{
    public class RecordTypeMap : Attribute
    {
        public string RecordType { get; set; }

        public RecordTypeMap(string recordType)
        {
            RecordType = recordType;
        }
    }
}
