using System;
using System.Collections.Generic;
using System.Linq;
using JosephM.Record.Extentions;
using JosephM.Record.IService;
using System.Reflection;
using JosephM.Core.Attributes;
using JosephM.Core.Extentions;

namespace JosephM.Record.Service
{
    /// <summary>
    ///     Implementation Of IRecord For A Standard CLR Object For Use By The ObjectRecordService Implementation Of
    ///     IRecordService
    /// </summary>
    public class ObjectRecord : RecordBase
    {
        public ObjectRecord(object instance)
            : base(instance.GetType().AssemblyQualifiedName)
        {
            Instance = instance;
        }

        public override string Id
        {
            get
            {
                var propInfo = GetKeyPropertyInfo();
                if(propInfo != null)
                {
                    return Instance.GetPropertyValue(propInfo.Name)?.ToString();
                }
                return null;
            }
            set
            {
                var propInfo = GetKeyPropertyInfo();
                if (propInfo != null)
                {
                    Instance.SetPropertyValue(propInfo.Name, value);
                }
            }
        }

        private PropertyInfo GetKeyPropertyInfo()
        {
            var properties = Instance.GetType().GetAllPropertyInfos(); ;
            var keyProperties = properties.Where(p => p.GetCustomAttribute<KeyAttribute>() != null).ToArray();
            return keyProperties.Any() ? keyProperties.First() : null;
        }

        public object Instance { get; set; }

        public override void SetField(string field, object value, IRecordService service)
        {
            var parseFieldResponse =
                service.ParseFieldRequest(new ParseFieldRequest(field, Type, value));
            if (parseFieldResponse.Success)
                Instance.GetType()
                    .GetProperty(field)
                    .GetSetMethod()
                    .Invoke(Instance, new[] {parseFieldResponse.ParsedValue});
            else
                throw new ArgumentOutOfRangeException(parseFieldResponse.Error);
        }

        public override object GetField(string field)
        {
            return field == "ToString"
                ? Instance.ToString()
                : Instance.GetType().GetProperty(field).GetGetMethod().Invoke(Instance, null);
        }

        public override bool ContainsField(string field)
        {
            return GetFieldsInEntity().Contains(field);
        }

        public override IEnumerable<string> GetFieldsInEntity()
        {
            return Instance.GetType().GetProperties().Select(s => s.Name);
        }

        public override void SetField(string field, object value)
        {
            Instance.GetType()
                .GetProperty(field)
                .GetSetMethod()
                .Invoke(Instance, new[] {value});
        }
    }
}