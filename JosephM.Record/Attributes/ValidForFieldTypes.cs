using JosephM.Record.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JosephM.Record.Attributes
{
    public class ValidForFieldTypes : Attribute
    {
        public IEnumerable<RecordFieldType> FieldTypes { get; set; }

        public ValidForFieldTypes(params RecordFieldType[] fieldTypes)
        {
            FieldTypes = fieldTypes;
        }
    }
}
