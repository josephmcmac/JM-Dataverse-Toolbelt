using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Record.Attributes
{
    public class RecordFieldMap : RecordPropertyMap
    {
        public string FieldName { get; set; }

        public RecordFieldMap(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}
