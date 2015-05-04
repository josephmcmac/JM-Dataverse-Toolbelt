using System.Collections.Generic;
using JosephM.Record.IService;

namespace JosephM.Record.Service
{
    /// <summary>
    ///     General Use Implementation Of IRecord For Use By The RecordService Implementation Of IRecordService
    /// </summary>
    public class RecordObject : RecordBase
    {
        private readonly SortedDictionary<string, object> _fields = new SortedDictionary<string, object>();

        public RecordObject(string entityType) : base(entityType)
        {
        }

        public override void SetField(string field, object value, IRecordService service)
        {
            SetField(field, value);
        }

        public override void SetField(string field, object value)
        {
            if (_fields.ContainsKey(field))
                _fields[field] = value;
            else
                _fields.Add(field, value);
        }

        public override object GetField(string field)
        {
            //is there a safer way to do this
            if (_fields.ContainsKey(field))
                return _fields[field];
            else
                return null;
        }

        public override bool ContainsField(string field)
        {
            return _fields.ContainsKey(field);
        }

        public override IEnumerable<string> GetFieldsInEntity()
        {
            return _fields.Keys;
        }

        public override IEnumerable<IRecord> GetActivityParties(string field)
        {
            var fieldValue = GetField(field);
            return fieldValue == null ? new IRecord[0] : (IEnumerable<IRecord>) fieldValue;
        }
    }
}