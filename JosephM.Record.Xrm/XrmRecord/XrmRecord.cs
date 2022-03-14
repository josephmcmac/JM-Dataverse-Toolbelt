using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using JosephM.Record.Service;

namespace JosephM.Record.Xrm.XrmRecord
{
    [DataContract]
    public class XrmRecord : RecordBase, IRecord
    {
        public XrmRecord(string recordType)
            : base(recordType)
        {
        }

        public IEnumerable<object> LoadedFields
        {
            get { return _fields.Values; }
        }

        private readonly SortedDictionary<string, object> _fields = new SortedDictionary<string, object>();

        public SortedDictionary<string, object> Fields
        {
            get { return _fields; }
        }

        public override void SetField(string field, object value, IRecordService service)
        {
            var parseFieldResponse =
                service.ParseFieldRequest(new ParseFieldRequest(field, Type, value));
            if (parseFieldResponse.Success)
            {
                SetField(field, parseFieldResponse.ParsedValue);
                if (field == service.GetPrimaryKey(Type))
                    Id = GetField(field).ToString();
            }
            else
            {
                throw new ArgumentException(string.Concat("Error parsing field: ", parseFieldResponse.Error));
            }
        }

        public override object GetField(string field)
        {
            if (Fields.ContainsKey(field))
                return Fields[field];
            return null;
        }

        public override bool ContainsField(string field)
        {
            return Fields.ContainsKey(field);
        }

        public override IEnumerable<string> GetFieldsInEntity()
        {
            return Fields.Keys;
        }

        public override void SetField(string fieldName, object value)
        {
            if (Fields.ContainsKey(fieldName))
                Fields[fieldName] = value;
            else
                Fields.Add(fieldName, value);
        }
    }
}