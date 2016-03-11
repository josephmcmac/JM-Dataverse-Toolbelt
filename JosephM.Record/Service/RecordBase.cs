#region

using JosephM.Core.FieldType;
using JosephM.Record.IService;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

#endregion

namespace JosephM.Record.Service
{
    /// <summary>
    ///     Base Class For An Implementation Of IRecord
    /// </summary>
    [Serializable]
    public abstract class RecordBase : IRecord
    {
        protected RecordBase(string entityType)
        {
            Type = entityType;
        }

        public string IdTargetFieldOverride { get; set; }

        public object this[string fieldName]
        {
            get { return GetField(fieldName); }
            set { SetField(fieldName, value); }
        }

        public string Type { get; set; }

        public abstract void SetField(string field, object value);

        public abstract void SetField(string field, object value, IRecordService service);

        public bool FieldIsEmpty(string field)
        {
            var result = false;
            var value = GetField(field);
            if (value == null)
                result = true;
            else if (value is string && (string)value == "")
                result = true;
            return result;
        }

        public abstract object GetField(string field);

        public virtual string GetStringField(string field)
        {
            var value = GetField(field);
            return value == null ? null : value.ToString();
        }

        public abstract bool ContainsField(string field);

        public bool? GetBoolFieldNullable(string field)
        {
            return (bool?)GetField(field);
        }

        public bool GetBoolField(string field)
        {
            return GetBoolFieldNullable(field) ?? false;
        }

        public string Id { get; set; }

        public int? GetIntegerFieldNullable(string fieldName)
        {
            var value = GetField(fieldName);
            return (int?)value;
        }

        public int GetIntegerField(string fieldName)
        {
            return GetIntegerFieldNullable(fieldName) ?? 0;
        }

        public virtual IEnumerable<string> GetFieldsInEntity()
        {
            throw new NotImplementedException();
        }

        public string GetLookupType(string fieldName)
        {
            var lookup = GetLookupField(fieldName);
            return lookup == null ? null : lookup.RecordType;
        }

        public virtual string GetLookupId(string fieldName)
        {
            var lookup = GetLookupField(fieldName);
            return lookup == null ? null : lookup.Id;
        }

        public virtual Lookup GetLookupField(string fieldName)
        {
            var fieldValue = GetField(fieldName);
            if (fieldValue == null)
                return null;
            if (fieldValue is Lookup)
                return (Lookup)fieldValue;
            throw new Exception(string.Format("Expected Field Of Type {0}", typeof(Lookup).Name));
        }

        public string GetLookupName(string fieldName)
        {
            var lookup = GetLookupField(fieldName);
            return lookup == null ? null : lookup.Name;
        }

        public void SetLookup(string fieldName, string id, string recordType)
        {
            SetField(fieldName, new Lookup(recordType, id, null));
        }

        public virtual string GetOptionKey(string fieldName)
        {
            var fieldValue = GetField(fieldName);
            if (fieldValue is PicklistOption)
                return ((PicklistOption)fieldValue).Key;
            if (fieldValue == null)
                return null;
            throw new NotImplementedException(string.Format("Type {0} not Implemented", fieldValue.GetType().Name));
        }

        public string GetName(IRecordService service)
        {
            return GetStringField(service.GetRecordTypeMetadata(Type).PrimaryFieldSchemaName);
        }

        public virtual IEnumerable<IRecord> GetActivityParties(string field)
        {
            var value = GetField(field);
            return value != null ? (IEnumerable<IRecord>)value : new IRecord[0];
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (var field in GetFieldsInEntity())
            {
                info.AddValue(field, GetField(field));
            }
        }

        public DateTime? GetDateTime(string field)
        {
            return (DateTime?)GetField(field);
        }
    }
}