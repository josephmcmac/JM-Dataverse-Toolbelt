using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Record.Attributes
{
    public class EnumerableRecordFieldMap : RecordPropertyMap
    {
        public string LookupField { get; set; }

        public EnumerableRecordFieldMap(string lookupField)
        {
            LookupField = lookupField;
        }
    }
}
