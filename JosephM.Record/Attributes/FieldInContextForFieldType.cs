using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Metadata;
using System.Collections.Generic;
using System.Linq;

namespace JosephM.Record.Attributes
{
    public  class FieldInContextForFieldType : PropertyInContext
    {
        public FieldInContextForFieldType(string otherFieldName, params RecordFieldType[] validTypes)
        {
            OtherFieldName = otherFieldName;
            ValidTypes = validTypes;
        }

        public string OtherFieldName { get; }
        public IEnumerable<RecordFieldType> ValidTypes { get; }

        public override bool IsInContext(IRecordService recordService, IRecord record)
        {
            return ValidTypes.Contains(recordService.GetFieldType(OtherFieldName, record.Type));
        }
    }
}
